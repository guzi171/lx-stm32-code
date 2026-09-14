/**
 ******************************************************************************
 * @author  guzi
 * @date    2026-09-07
 * @brief   LED相关处理文件
 *
 * @par 主要功能
 * - 控制LED状态
 *
 * @note
 * - 使用注意事项
 * - 无
 ******************************************************************************
 */

 #include "bsp_led.h"


/**
 * @brief 控制 LED 点亮、熄灭或状态翻转
 * @param[in] operation LED 操作类型
 */
void led_control(led_operation_t operation)
{
    switch (operation)
    {
        case LED_ON:
            HAL_GPIO_WritePin(LED_GPIO_Port, LED_Pin, GPIO_PIN_RESET);
            break;

        case LED_OFF:
            HAL_GPIO_WritePin(LED_GPIO_Port, LED_Pin, GPIO_PIN_SET);
            break;

        case LED_TOGGLE:
            HAL_GPIO_TogglePin(LED_GPIO_Port, LED_Pin);
            break;

        default:
            break;
    }
}
