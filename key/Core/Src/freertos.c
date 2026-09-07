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
#include "queue.h"
#include "bsp_led.h"
/* USER CODE END Includes */

/* Private typedef -----------------------------------------------------------*/
/* USER CODE BEGIN PTD */

/* USER CODE END PTD */

/* Private define ------------------------------------------------------------*/
/* USER CODE BEGIN PD */

/* USER CODE END PD */

/* Private macro -------------------------------------------------------------*/
/* USER CODE BEGIN PM */

/* USER CODE END PM */

/* Private variables ---------------------------------------------------------*/
/* USER CODE BEGIN Variables */

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

//********************** 任务相关 **********************//

osThreadId_t keyTaskHandle;
const osThreadAttr_t key_attributes = {
  .name = "keyTask",
  .stack_size = 128 * 4,
  .priority = (osPriority_t) osPriorityNormal,
};

void key_task(void *argument);

osThreadId_t ledTaskHandle;

const osThreadAttr_t led_attributes = {
    .name = "ledTask",
    .stack_size = 128 * 4,
    .priority = (osPriority_t)osPriorityNormal,
};

void led_task(void *argument);


//********************** 任务相关 **********************//


//********************** 队列相关 **********************//

QueueHandle_t key_queue;
QueueHandle_t led_queue;

//********************** 队列相关 **********************//


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

	key_queue = xQueueCreate(10, sizeof(uint32_t));
	led_queue = xQueueCreate(10, sizeof(led_operation_t));

	if ((key_queue == NULL) || (led_queue == NULL))
	{
		Error_Handler();
	}

  /* USER CODE END RTOS_QUEUES */

  /* Create the thread(s) */
  /* creation of defaultTask */
  defaultTaskHandle = osThreadNew(StartDefaultTask, NULL, &defaultTask_attributes);

  /* USER CODE BEGIN RTOS_THREADS */
  /* add threads, ... */
  keyTaskHandle = osThreadNew(key_task, NULL, &key_attributes);
  ledTaskHandle = osThreadNew(led_task, NULL, &led_attributes);
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
  uint32_t r_data;
  led_operation_t operation = LED_TOGGLE;
  /* Infinite loop */
  for(;;)
  {
		if (uxQueueMessagesWaiting(key_queue) > 0) // 需要用判断是否有数据来判断队列
		{
			if (pdPASS == xQueueReceive(key_queue, &r_data, 100))
			{
				printf("r_data = [%d]\r\n", r_data);
				xQueueSend(led_queue, &operation, portMAX_DELAY);
			}
		}

		printf("StartDefaultTask...\r\n");
		osDelay(100);
  }
  /* USER CODE END StartDefaultTask */
}

/* Private application code --------------------------------------------------*/
/* USER CODE BEGIN Application */

// key任务
void key_task(void *argument)
{
    key_press_state_t key_state = KEY_NOT_PRESS;	// 按键状态
	// key_queue = xQueueCreate(10, sizeof(uint32_t));	// 按键发送队列
	// if (NULL == key_queue)
	// {
	// 	printf("create key_queue file\r\n");
	// }
	uint32_t count_tick = 0; 

    for(;;)
    {
		count_tick++;
		key_state = key_scan();

        if (KEY_PRESS == key_state)        			 // 按键被按下
        {
            printf("KEY_PRESS\r\n");
			if (pdPASS == xQueueSend(key_queue, &count_tick, 0))
			{
				printf("send key_queue succeed\r\n");
			}
        }
        else if (KEY_NOT_PRESS == key_state) 		// 按键未被按下
        {
            printf("KEY_NOT_PRESS\r\n");
        }


        printf("key_task...\r\n");
        osDelay(1000);
    }

}

// led任务
void led_task(void *argument)
{
    led_operation_t operation = LED_OFF;

	for (;;)
	{
		if (xQueueReceive(led_queue, &operation, portMAX_DELAY) == pdPASS)
		{
			led_control(operation);
		}
		osDelay(10);
	}
}


/* USER CODE END Application */

