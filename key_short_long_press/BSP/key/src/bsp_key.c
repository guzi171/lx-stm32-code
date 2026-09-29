/**
 *******************************************************************************
 * @file	bsp_key.c
 * @author  guzi
 * @date    2026-09-27
 * @brief   按键相关处理函数
 *
 * @par 主要功能
 * - 读取按键是否按下
 * - 判断按键为长按还是短按
 *
 * @note 使用注意事项
 * - 1 tab == 4 spaces
 *******************************************************************************
 */

#include "bsp_key.h"

/*============================== Includes ====================================*/
#include "FreeRTOS.h"
#include "task.h"
#include "queue.h"
/*============================ End of Includes ===============================*/

/*========================== Type Definitions ================================*/
extern QueueHandle_t  key_irq_queue;
/*======================== End of Type Definitions ===========================*/

/*======================== Function Declarations =============================*/

/*====================== End of Function Declarations ========================*/


/**
 * @brief  读取按键是否按下的状态
 * @return key_press_state_t
 * 				KEY_NOT_PRESS :	按键未按下
 * 				KEY_PRESS     :	按键按下
 * @note   函数可以进行单次检测，未进行消抖处理
 */
key_press_state_t key_press_state(void)
{
	if (GPIO_PIN_RESET == HAL_GPIO_ReadPin(KEY_GPIO_Port, KEY_Pin))
	{
		return KEY_PRESS;
	}
	return KEY_NOT_PRESS;
} 

/**
 * @brief  判断按键事件，当前为短按和长按
 * @param  wait_time 判断长按按下事件 (单位：ms)
 * @return key_event_t
 * 				KEY_EVENT_NONE  ：按键未按
 * 				KEY_SHORT_PRESS ：按键短按
 * 				KEY_LONG_PRESS  ：按键长按
 * @note   FreeRTOS中，该函数的while会阻塞（后续可以优化）
 */
key_event_t key_press_event(uint32_t wait_time)
{
	key_press_state_t key_press = KEY_NOT_PRESS;
	uint32_t         press_tick = 0;	// 按键按下时，第一次记录的时间
	
	/* 第一次判断按键的值 */
	key_press = key_press_state();
	if (KEY_PRESS == key_press) 		// 按键按下
	{
		press_tick = HAL_GetTick();
		while ((press_tick + wait_time) > HAL_GetTick());
		
		/* 第二次判断按键的值 */
		key_press = key_press_state();
		if (KEY_NOT_PRESS == key_press)
		{
			return KEY_SHORT_PRESS;
		}
		else 
		{
			// 加上这句while只执行一次长按 （松手后执行）
			// 不加这句while就是一直执行长按返回
			while (KEY_PRESS == key_press_state()); 
			return KEY_LONG_PRESS;
		}
	}
	
	return KEY_EVENT_NONE;
}

/**
 * @brief  回调函数
 * @param  GPIO_Pin 触发引脚
 * @note   切换触发沿
 */
void HAL_GPIO_EXTI_Callback(uint16_t GPIO_Pin)
{
	static key_edge_t now_edge 	= KEY_EDGE_FALLING;	// 当前触发沿
	static key_edge_t next_edge = KEY_EDGE_FALLING;	// 下次触发沿
	GPIO_InitTypeDef GPIO_InitStruct = {0};

	if ((GPIO_Pin != KEY_Pin) || (key_irq_queue == NULL))
	{
		return ;
	}

	GPIO_InitStruct.Pin 	= KEY_Pin;
	GPIO_InitStruct.Pull 	= GPIO_PULLUP;

	/* 1. 下降沿（按键按下） */
	if (KEY_EDGE_FALLING == now_edge)				
	{
		key_irq_edge_t key_edge_temp1 = 
		{
			.edge_now  = KEY_EDGE_FALLING,
			.edge_tick = HAL_GetTick(),
		};

		// 切换下次为上升沿触发
		next_edge = KEY_EDGE_RISING;
		GPIO_InitStruct.Mode = GPIO_MODE_IT_RISING;
		HAL_GPIO_Init(KEY_GPIO_Port, &GPIO_InitStruct);

		xQueueSendFromISR(key_irq_queue, &key_edge_temp1, 0);
	}
	/* 2. 上升沿（按键松开） */
	else if (KEY_EDGE_RISING == now_edge)			
	{
		key_irq_edge_t key_edge_temp2 = 
		{
			.edge_now  = KEY_EDGE_RISING,
			.edge_tick = HAL_GetTick(),
		};

		// 切换下次为下降沿触发
		next_edge = KEY_EDGE_FALLING;
		GPIO_InitStruct.Mode = GPIO_MODE_IT_FALLING;
		HAL_GPIO_Init(KEY_GPIO_Port, &GPIO_InitStruct);

		xQueueSendFromISR(key_irq_queue, &key_edge_temp2, 0);
	}
	now_edge = next_edge;
}
