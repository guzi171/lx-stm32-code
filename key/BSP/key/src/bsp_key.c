/**
 ******************************************************************************
 * @author  guzi
 * @date    2026-09-06
 * @brief   按键相关处理文件
 *
 * @par 主要功能
 * - 判断按键是否按下
 *
 * @note
 * - 使用注意事项
 * - 无
 ******************************************************************************
 */

#include "bsp_key.h"

/**
 * @brief  读取按键当前状态
 *
 * 读取 KEY 引脚的电平。当引脚为低电平时认为按键按下，
 * 否则认为按键未按下。
 *
 * @return key_press_state_t 按键状态
 * @retval KEY_PRESS     按键已按下
 * @retval KEY_NOT_PRESS 按键未按下
 *
 * @note 本函数只进行一次电平检测，不包含按键消抖和松手检测。
 */
key_press_state_t key_scan(void)
{
    if (GPIO_PIN_RESET == HAL_GPIO_ReadPin(KEY_GPIO_Port, KEY_Pin))
    {
        return KEY_PRESS;
    }
    return KEY_NOT_PRESS;
}
