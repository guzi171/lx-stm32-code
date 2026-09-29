/**
 *******************************************************************************
 * @file    my_debug.h
 * @author  guzi
 * @date    2026-09-17
 * @brief   printf带行号和函数调试
 *
 * @par 主要功能
 * - printf_log：	功能等同于printf
 * - printf_debug：	功能等同于printf + 函数 + 行号
 *
 * @note 使用注意事项
 * - 1 tab == 4 spaces
 *******************************************************************************
 */

#ifndef __MY_DEBUG_H
#define __MY_DEBUG_H

/*============================== Includes ====================================*/
#include <stdio.h>
/*============================ End of Includes ===============================*/

/*========================== Type Definitions ================================*/
/* 调试日志开关 */
#define DEBUG_LOG 1 // 0 无打印  1 打印	

#if DEBUG_LOG
	#define 	printf_log(...) 						\
					printf(__VA_ARGS__)
	#define 	printf_debug(fmt, ...) 					\
					printf("[DEBUG][%s:%d]  " fmt, 			\
						__FUNCTION__, 						\
						__LINE__,     						\
						##__VA_ARGS__	)
#else
	#define 	printf_log(...)			((void)0)
	#define 	printf_debug(...)		((void)0)
#endif /* DEBUG_LOG */

/*======================== End of Type Definitions ===========================*/

/*======================== Function Declarations =============================*/

/*====================== End of Function Declarations ========================*/

#endif /* __MY_DEBUG_H */
