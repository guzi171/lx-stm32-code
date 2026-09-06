#ifndef __BSP_KEY_H
#define __BSP_KEY_H

#include "main.h"

// 按键按下状态
typedef enum
{
    KEY_NOT_PRESS   = 0,    // 按键未按下
    KEY_PRESS       = 1,    // 按键按下
} key_press_state_t;

key_press_state_t key_scan(void);

#endif // end __BSP_KEY_H
