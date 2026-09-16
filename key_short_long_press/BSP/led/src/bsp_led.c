/**
 *******************************************************************************
 * @file	bsp_led.c
 * @author  guzi
 * @date    2026-09-13
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

/*======================== Function Declarations =============================*/
static void led_ficker(void);
/*====================== End of Function Declarations ========================*/

/**
 * @brief  控制LED状态
 * @param  led_state
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
	case LED_TICKER:
		led_ficker();	
		break;
	default:
		break;
	}
}

/**
 * @brief  实现LED闪烁--三次
 * @param  参数名 参数的含义
 * @note   写成亮灭三次，相对于写翻转6次，能更加精确的显示闪烁三次
 */
static void led_ficker(void)
{
    for (uint8_t i = 0; i < 3; i++)
    {
        HAL_GPIO_WritePin(LED_GPIO_Port, LED_Pin, GPIO_PIN_RESET);
        osDelay(200);

        HAL_GPIO_WritePin(LED_GPIO_Port, LED_Pin, GPIO_PIN_SET);
        osDelay(200);
    }
}
