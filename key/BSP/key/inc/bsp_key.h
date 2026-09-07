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

#ifndef __BSP_KEY_H
#define __BSP_KEY_H

/*============================== Includes ====================================*/

#include "main.h"

/*============================ End of Includes ===============================*/


/*========================== Type Definitions ================================*/

/**
 * @brief 按键状态
 */
typedef enum
{
    KEY_NOT_PRESS = 0,    /**< 按键未按下 */
    KEY_PRESS     = 1,    /**< 按键已按下 */
} key_press_state_t;

/*======================== End of Type Definitions ===========================*/


/*======================== Function Declarations =============================*/

/**
 * @brief  读取按键当前状态
 *
 * @return key_press_state_t 按键状态
 * @retval KEY_PRESS     按键已按下
 * @retval KEY_NOT_PRESS 按键未按下
 *
 * @note 本函数只读取一次按键引脚，不包含按键消抖。
 */
key_press_state_t key_scan(void);

/*====================== End of Function Declarations ========================*/

#endif /* __BSP_KEY_H */
