/**
 * @brief  按键扫描检测
 * @note   该函数应该10ms执行一次
 */
void key_scan(void)
{
    static uint32_t tick = 0;   // 按键扫描计时
    if (0 == key_state_t)       // 按键状态为低电平（按下）
    {
        tick++;
    }
    if (1 == key_state_t)      // 按键状态为高电平（按键释放/按键没有按下/抖动）
    {
        if (tick >= 100)       // 按下超过1秒
        {
            // 执行长按代码
        }
        else if (tick > 2)    // 按下超过30ms，但小于1秒  
        {
            // 执行短按代码
        }
        tick = 0;
    }
}



