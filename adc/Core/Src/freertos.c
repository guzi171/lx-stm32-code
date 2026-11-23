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

#include "queue.h"
/* USER CODE END Includes */

/* Private typedef -----------------------------------------------------------*/
/* USER CODE BEGIN PTD */

/* USER CODE END PTD */

/* Private define ------------------------------------------------------------*/
/* USER CODE BEGIN PD */
#define BUFFER_SIZE	1

uint32_t *buffer1 = NULL;
uint32_t *buffer2 = NULL;

QueueHandle_t xQueue1 = NULL;
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

// V1--申请4字节的内存
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

// V2--打印查看
	printf("buffer1 = 0x%x  buffer2 = 0x%x \r\n", 
		buffer1,
		buffer2
	);
	printf("buffer1[0] = 0x%X, buffer2[0] = 0x%X\r\n",
		buffer1[0],
        buffer2[0]);


	xQueue1 = xQueueCreate(10, sizeof(uint32_t));
	if (NULL == xQueue1)
	{
		printf("create queue fail\r\n");
		return ;
	}

	// 启动ADC转换
	// &hadc1  : adc句柄地址
	// buffer1 : dma搬运数据地址
	// BUFFER_SIZE : 搬运多少数据项
	if (HAL_OK != HAL_ADC_Start_DMA(&hadc1, buffer1, BUFFER_SIZE))
	{
		printf("adc dma failed\r\n");
		return ;
	}

// V3--实现队列脚手架
	// xQueue1 = xQueueCreate(10, sizeof(uint32_t));
	// if (NULL == xQueue1)
	// {
	// 	printf("create queue fail\r\n");
	// 	return ;
	// }

	// uint32_t send_test1 = 123;
	// BaseType_t xRx_Status1 = xQueueSendToBack(xQueue1, &send_test1, 0);
	// printf("xRx_Status1:%d\r\n", xRx_Status1);
	
	// uint32_t rx_test1 = 456;
	// BaseType_t xRx_Status2 = xQueueReceive(xQueue1, &rx_test1, 0);
	// printf("xRx_Status2:%d\r\n", xRx_Status2);
	
	// printf("rx_test1:%d\r\n", rx_test1);




  /* Infinite loop */
  for(;;)
  {
//	  printf("hello world\r\n");

	uint32_t rx_test2 = 0;
	if (pdPASS == xQueueReceive(xQueue1, &rx_test2, portMAX_DELAY))
	{
		printf("rx_test2: 0x%x\r\n", rx_test2);
	}


	osDelay(1000);
  }
  /* USER CODE END StartDefaultTask */
}

/* Private application code --------------------------------------------------*/
/* USER CODE BEGIN Application */


void HAL_ADC_ConvCpltCallback(ADC_HandleTypeDef *hadc)
{
	// printf("buffer = [%d]\r\n", buffer1[0]);

	BaseType_t xSend2 	= pdFALSE;
	uint32_t send2_flag = 0xAA; 	// 标志位

	xQueueSendToBackFromISR(xQueue1, &send2_flag, &xSend2);
	portYIELD_FROM_ISR(xSend2);
}


void HAL_ADC_ErrorCallback(ADC_HandleTypeDef *hadc)
{
	printf("adc fail\r\n");
}



/* USER CODE END Application */

