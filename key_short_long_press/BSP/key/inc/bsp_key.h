/**
 *******************************************************************************
 * @file	bsp_key.h
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

#ifndef __BSP_KEY_H
#define __BSP_KEY_H

/*============================== Includes ====================================*/
#include "main.h"
/*============================ End of Includes ===============================*/

/*========================== Type Definitions ================================*/
/* 枚举变量：按键按下状态 */
typedef enum
{
	KEY_NOT_PRESS = 0,		// 按键未按下
	KEY_PRESS     = 1,		// 按键已按下
} key_press_state_t;

/* 枚举变量：按键按下事件 */
typedef enum
{
	KEY_EVENT_NONE  = 0,	// 按键未按
	KEY_SHORT_PRESS = 1,	// 按键短按
	KEY_LONG_PRESS  = 2,	// 按键长按
} key_event_t;
/*======================== End of Type Definitions ===========================*/

/*======================== Function Declarations =============================*/
key_press_state_t key_press_state(void);			// 判断按键按下的状态
key_event_t key_press_event(uint32_t wait_time);	// 判断按键按下的事件
/*====================== End of Function Declarations ========================*/

#endif  /* __BSP_KEY_H */
