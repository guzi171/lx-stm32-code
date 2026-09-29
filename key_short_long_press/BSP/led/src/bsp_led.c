/**
 *******************************************************************************
 * @file	bsp_led.c
 * @author  guzi
 * @date    2026-09-29
 * @brief   LED相关处理函数
 *
 * @par 主要功能
 * - 控制LED状态：关闭、打开、翻转、闪烁
 *
 * @note 使用注意事项
 * - 1 tab == 4 spaces
 *******************************************************************************
 */

#include "bsp_led.h"
#include "cmsis_os.h"

/*============================== Includes ====================================*/

/*============================ End of Includes ===============================*/

/*========================== Type Definitions ================================*/
static volatile uint8_t g_led_flicker_cnt   = 0;	/* led剩余闪烁次数 */
static volatile uint8_t g_led_flicker_state = 0;	/* led当前闪烁状态(0灭1亮) */ 
/*======================== End of Type Definitions ===========================*/

/*======================== Function Declarations =============================*/
static void led_flicker_3(void);
/*====================== End of Function Declarations ========================*/

/**
 * @brief  控制LED状态
 * @param  led_state
 * 				LED_ON 		// LED打开
 *				LED_OFF		// LED关闭
 *				LED_TOGGLE 	// LED翻转
 *				LED_TICKER	// LED闪烁（三次）
 */
void led_state_fun(led_state_t led_state)
{
	switch (led_state)
	{
	case LED_OFF:
		HAL_GPIO_WritePin(LED_GPIO_Port, LED_Pin, GPIO_PIN_SET);
		break;
	case LED_ON:
		HAL_GPIO_WritePin(LED_GPIO_Port, LED_Pin, GPIO_PIN_RESET);
		break;
	case LED_TOGGLE:
		HAL_GPIO_TogglePin(LED_GPIO_Port, LED_Pin);
		break;		
	case LED_TICKER:
		led_flicker_3();	
		break;
	default:
		break;
	}
}

/**
 * @brief  实现LED闪烁--三次
 * @note   写成亮灭三次，相对于写翻转6次，能更加精确的显示闪烁三次
 */
static void led_flicker_3(void)
{
    for (uint8_t i = 0; i < 3; i++)
    {
        HAL_GPIO_WritePin(LED_GPIO_Port, LED_Pin, GPIO_PIN_RESET);
        osDelay(200);

        HAL_GPIO_WritePin(LED_GPIO_Port, LED_Pin, GPIO_PIN_SET);
        osDelay(200);
    }
}

/**
 * @brief  led闪烁函数
 * @param  ficker_cnt led闪烁次数
 * @note   ficker_cnt范围为0-255，调用需要注意
 */
void led_flicker(uint8_t ficker_cnt)
{
	// 闪烁之前固定将led状态关闭
	led_state_fun(LED_OFF);
	g_led_flicker_state = 0;

	g_led_flicker_cnt 	= ficker_cnt;
}

/**
 * @brief  定时器2--led回调函数
 * @param  参数名 参数的含义
 * @return 返回值的含义
 * @note   100ms执行一次
 */
void tim_led_callback(void)
{
	if (0 == g_led_flicker_cnt)
		return ;

	if (0 == g_led_flicker_state)
	{
		led_state_fun(LED_ON);
		g_led_flicker_state = 1;
	}
	else if (1 == g_led_flicker_state)
	{
		led_state_fun(LED_OFF);
		g_led_flicker_state = 0;

		g_led_flicker_cnt--;
	}
}
