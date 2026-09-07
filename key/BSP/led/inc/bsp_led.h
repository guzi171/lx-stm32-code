/**
 ******************************************************************************
 * @author  guzi
 * @date    2026-09-07
 * @brief   LED 相关处理文件
 *
 * @par 主要功能
 * - 控制 LED 点亮、熄灭和状态翻转
 *
 * @note
 * - 使用注意事项
 * - 无
 ******************************************************************************
 */

#ifndef __BSP_LED_H
#define __BSP_LED_H

/*============================== Includes ====================================*/

#include "main.h"

/*============================ End of Includes ===============================*/


/*========================== Type Definitions ================================*/

/**
 * @brief LED 操作类型
 */
typedef enum
{
    LED_ON     = 0,    /**< 点亮 LED */
    LED_OFF    = 1,    /**< 熄灭 LED */
    LED_TOGGLE = 2,    /**< 翻转 LED 当前状态 */
} led_operation_t;

/*======================== End of Type Definitions ===========================*/


/*======================== Function Declarations =============================*/

/**
 * @brief  执行指定的 LED 操作
 *
 * @param[in] operation LED 操作类型，可选择点亮、熄灭或状态翻转
 *
 * @note LED 为低电平点亮、高电平熄灭。
 */
void led_control(led_operation_t operation);

/*====================== End of Function Declarations ========================*/

#endif /* __BSP_LED_H */

