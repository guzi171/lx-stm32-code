/**
 *******************************************************************************
 * @file	bsp_key.c
 * @author  guzi
 * @date    2026-09-13
 * @brief   模块功能简介
 *
 * @par 主要功能
 * - 功能一
 * - 功能二
 * - 功能三
 *
 * @note 使用注意事项
 * - 1 tab == 4 spaces
 *******************************************************************************
 */

#include "bsp_key.h"

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
 * @param  wait_time 判断长按按下事件
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
			// 加上while只执行一次长按，松手后执行
			// while (KEY_PRESS == key_press_state()); 
			return KEY_LONG_PRESS;
		}
	}
	
	return KEY_EVENT_NONE;
}


