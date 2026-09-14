/**
 *******************************************************************************
 * @file	bsp_led.h
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

#ifndef __BSP_LED_H
#define __BSP_LED_H

/*============================== Includes ====================================*/
#include "main.h"
/*============================ End of Includes ===============================*/

/*========================== Type Definitions ================================*/
/* 枚举变量：LED状态 */
typedef enum
{
	LED_ON 		= 0,	// LED打开
	LED_OFF		= 1,	// LED关闭
	LED_TOGGLE 	= 2,	// LED翻转
} led_state_t;
/*======================== End of Type Definitions ===========================*/

/*======================== Function Declarations =============================*/
void led_state_fun(led_state_t led_state);	// 控制LED的状态
/*====================== End of Function Declarations ========================*/

#endif /* __BSP_LED_H */
