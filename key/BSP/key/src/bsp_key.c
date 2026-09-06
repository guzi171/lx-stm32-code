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


key_press_state_t key_scan(void)
{
    if (GPIO_PIN_RESET == HAL_GPIO_ReadPin(KEY_GPIO_Port, KEY_Pin))
    {
        return KEY_PRESS;
    }
    return KEY_NOT_PRESS;
}
