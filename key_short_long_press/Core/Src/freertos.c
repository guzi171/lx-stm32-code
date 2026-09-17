/* USER CODE BEGIN Header */
/**
  ******************************************************************************
  * File Name          : freertos.c
  * Description        : Code for freertos applications
  ******************************************************************************
  * @attention
  *
  * Copyright (c) 2026 STMicroelectronics.
  * All rights reserved.
  *
  * This software is licensed under terms that can be found in the LICENSE file
  * in the root directory of this software component.
  * If no LICENSE file comes with this software, it is provided AS-IS.
  *
  ******************************************************************************
  */
/* USER CODE END Header */

/* Includes ------------------------------------------------------------------*/
#include "FreeRTOS.h"
#include "task.h"
#include "main.h"
#include "cmsis_os.h"

/* Private includes ----------------------------------------------------------*/
/* USER CODE BEGIN Includes */
#include <stdio.h>
#include "bsp_key.h"
#include "bsp_led.h"
#include "queue.h"
/* USER CODE END Includes */

/* Private typedef -----------------------------------------------------------*/
/* USER CODE BEGIN PTD */

/* USER CODE END PTD */

/* Private define ------------------------------------------------------------*/
/* USER CODE BEGIN PD */
#define key_wait_time 1000	// 按键判断为长按时间 (单位：毫秒)
/* USER CODE END PD */

/* Private macro -------------------------------------------------------------*/
/* USER CODE BEGIN PM */

/* USER CODE END PM */

/* Private variables ---------------------------------------------------------*/
/* USER CODE BEGIN Variables */
/* FreeRTOS key 任务 */
osThreadId_t key_task_handle; // key 任务句柄
const osThreadAttr_t key_task_attributes = {
	.name = "key_task",
	.stack_size = 128 * 2,
	.priority = (osPriority_t) osPriorityHigh,
}; 

/* FreeRTOS led 任务 */
osThreadId_t led_task_handle; // led 任务句柄
const osThreadAttr_t led_task_attributes = {
	.name = "led_task",
	.stack_size = 128 * 2,
	.priority = (osPriority_t) osPriorityRealtime,
}; 

/* 按键事件队列 */
osMessageQueueId_t key_event_queue = NULL;
/* USER CODE END Variables */
/* Definitions for defaultTask */
osThreadId_t defaultTaskHandle;
const osThreadAttr_t defaultTask_attributes = {
  .name = "defaultTask",
  .stack_size = 128 * 4,
  .priority = (osPriority_t) osPriorityNormal,
};

/* Private function prototypes -----------------------------------------------*/
/* USER CODE BEGIN FunctionPrototypes */
void key_task(void *argument);
void led_task(void *argument);
/* USER CODE END FunctionPrototypes */

void StartDefaultTask(void *argument);

void MX_FREERTOS_Init(void); /* (MISRA C 2004 rule 8.1) */

/**
  * @brief  FreeRTOS initialization
  * @param  None
  * @retval None
  */
void MX_FREERTOS_Init(void) {
  /* USER CODE BEGIN Init */

  /* USER CODE END Init */

  /* USER CODE BEGIN RTOS_MUTEX */
  /* add mutexes, ... */
  /* USER CODE END RTOS_MUTEX */

  /* USER CODE BEGIN RTOS_SEMAPHORES */
  /* add semaphores, ... */
  /* USER CODE END RTOS_SEMAPHORES */

  /* USER CODE BEGIN RTOS_TIMERS */
  /* start timers, add new ones, ... */
  /* USER CODE END RTOS_TIMERS */

  /* USER CODE BEGIN RTOS_QUEUES */
  /* add queues, ... */
  key_event_queue = osMessageQueueNew(5, sizeof(key_event_t), NULL);
  /* USER CODE END RTOS_QUEUES */

  /* Create the thread(s) */
  /* creation of defaultTask */
  defaultTaskHandle = osThreadNew(StartDefaultTask, NULL, &defaultTask_attributes);

  /* USER CODE BEGIN RTOS_THREADS */
  /* add threads, ... */
  key_task_handle = osThreadNew(key_task, NULL, &key_task_attributes);
  led_task_handle = osThreadNew(led_task, NULL, &led_task_attributes);

  /* USER CODE END RTOS_THREADS */

  /* USER CODE BEGIN RTOS_EVENTS */
  /* add events, ... */
  /* USER CODE END RTOS_EVENTS */

}

/* USER CODE BEGIN Header_StartDefaultTask */
/**
  * @brief  Function implementing the defaultTask thread.
  * @param  argument: Not used
  * @retval None
  */
/* USER CODE END Header_StartDefaultTask */
void StartDefaultTask(void *argument)
{
  /* USER CODE BEGIN StartDefaultTask */
  /* Infinite loop */
  for(;;)
  {
    osDelay(1);
  }
  /* USER CODE END StartDefaultTask */
}

/* Private application code --------------------------------------------------*/
/* USER CODE BEGIN Application */

/**
 * @brief  FreeRTOS任务--按键处理
 * @note   - 判断长按还是短按 
 */
void key_task(void *argument)
{
	key_event_t key_event = KEY_EVENT_NONE;

	for (;;)
	{
		key_event = key_press_event(key_wait_time);
		if (KEY_SHORT_PRESS == key_event)		// 按键短按
		{
			printf("key_event short\r\n");
			if (pdPASS == xQueueSend(key_event_queue, &key_event, 0))
			{
				printf("key_event short queue\r\n");
			}
		}
		else if (KEY_LONG_PRESS == key_event)	// 按键长按
		{
			printf("key_event long\r\n");
			if (pdPASS == xQueueSend(key_event_queue, &key_event, 0))
			{
				printf("key_event long queue\r\n");
			}
		}

		osDelay(100);
	}
}

/**
 * @brief  FreeRTOS任务--LED控制
 * @note   - 处理按键短按和长按事件
 */
void led_task(void *argument)
{
	key_event_t key_event_led = KEY_EVENT_NONE;
	for (;;)
	{
		xQueueReceive(key_event_queue, &key_event_led, portMAX_DELAY);
		printf("led_task rx data\r\n");
		if (KEY_SHORT_PRESS == key_event_led)		// 按键短按
		{
			led_state_fun(LED_TOGGLE);
		}
		else if (KEY_LONG_PRESS == key_event_led)	// 按键长按
		{
			led_state_fun(LED_TICKER);
		}

		osDelay(100);
	}
}


/* USER CODE END Application */

