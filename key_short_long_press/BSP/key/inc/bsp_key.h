/**
 *******************************************************************************
 * @file	bsp_key.h
 * @author  guzi
 * @date    2026-09-26
 * @brief   按键处理头文件
 *
 * @par 主要功能
 * - 定义枚举变量
 * - 按键相关处理函数
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
/* 枚举：按键按下状态 */
typedef enum
{
	KEY_NOT_PRESS = 0,		// 按键未按下
	KEY_PRESS     = 1,		// 按键已按下
} key_press_state_t;

/* 枚举：按键按下事件 */
typedef enum
{
	KEY_EVENT_NONE  = 0,	// 按键未按
	KEY_SHORT_PRESS = 1,	// 按键短按
	KEY_LONG_PRESS  = 2,	// 按键长按
} key_event_t;

/* 枚举：按键触发沿 */
typedef enum
{
	KEY_EDGE_RISING	 = 0,		// 按键松开（上升沿）
	KEY_EDGE_FALLING = 1,		// 按键按下 (下降沿)
} key_edge_t;


/* 结构体：按键触发沿时间 */
typedef struct
{
	key_edge_t 	edge_now;		// 当前触发沿的类型
	uint32_t	edge_tick;		// 触发时间
} key_irq_edge_t;

/*======================== End of Type Definitions ===========================*/

/*======================== Function Declarations =============================*/
key_press_state_t key_press_state(void);			// 判断按键按下的状态
key_event_t key_press_event(uint32_t wait_time);	// 判断按键按下的事件
/*====================== End of Function Declarations ========================*/

#endif  /* __BSP_KEY_H */
