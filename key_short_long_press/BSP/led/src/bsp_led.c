/**
 *******************************************************************************
 * @file	bsp_led.c
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

#include "bsp_led.h"

/**
 * @brief  控制LED状态
 * @param  led_state_t
 * 				LED_ON 		// LED打开
 *				LED_OFF		// LED关闭
 *				LED_TOGGLE 	// LED翻转
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
	default:
		break;
	}
}
