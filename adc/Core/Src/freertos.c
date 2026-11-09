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
#include <stdio.h>	// printf
#include <stdlib.h>	// malloc free
#include <string.h>	// memset memcpy strlen
/* USER CODE END Includes */

/* Private typedef -----------------------------------------------------------*/
/* USER CODE BEGIN PTD */

/* USER CODE END PTD */

/* Private define ------------------------------------------------------------*/
/* USER CODE BEGIN PD */
#define BUFFER_SIZE	1

uint32_t *buffer1 = NULL;
uint32_t *buffer2 = NULL;
/* USER CODE END PD */

/* Private macro -------------------------------------------------------------*/
/* USER CODE BEGIN PM */
extern ADC_HandleTypeDef hadc1;
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
  /* USER CODE END RTOS_QUEUES */

  /* Create the thread(s) */
  /* creation of defaultTask */
  defaultTaskHandle = osThreadNew(StartDefaultTask, NULL, &defaultTask_attributes);

  /* USER CODE BEGIN RTOS_THREADS */
  /* add threads, ... */
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
	buffer1 = (uint32_t *)malloc(sizeof(uint32_t) * BUFFER_SIZE);
	buffer2 = (uint32_t *)malloc(sizeof(uint32_t) * BUFFER_SIZE);

	if (NULL == buffer1)
	{
		printf("create buffer1 fail\r\n");
		return ;
	}
	if (NULL == buffer2)
	{
		printf("create buffer2 fail\r\n");
		return ;
	}
	printf("buffer1 and buffer2 create succed\r\n");

	memset(buffer1, 0xff, sizeof(uint32_t) * BUFFER_SIZE);
	memset(buffer2, 0xff, sizeof(uint32_t) * BUFFER_SIZE);
	// 打印查看
	printf("buffer1 = 0x%08lx  buffer2 = 0x%08lx \r\n", 
		(unsigned long)buffer1,
		(unsigned long)buffer2
	);
	printf("buffer1[0] = 0x%08lX, buffer2[0] = 0x%08lX\r\n",
       (unsigned long)buffer1[0],
       (unsigned long)buffer2[0]);

	if (HAL_OK != HAL_ADC_Start_DMA(&hadc1, buffer1, BUFFER_SIZE))
	{
		printf("adc dma failed\r\n");
		return ;
	}


  /* Infinite loop */
  for(;;)
  {
	  printf("hello world\r\n");
    osDelay(1000);
  }
  /* USER CODE END StartDefaultTask */
}

/* Private application code --------------------------------------------------*/
/* USER CODE BEGIN Application */


void HAL_ADC_ConvCpltCallback(ADC_HandleTypeDef *hadc)
{
	printf("buffer = [%d]\r\n", buffer1[0]);
}


void HAL_ADC_ErrorCallback(ADC_HandleTypeDef *hadc)
{
	printf("adc fail\r\n");
}



/* USER CODE END Application */

