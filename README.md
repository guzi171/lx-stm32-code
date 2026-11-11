# 代码实现

ADC1 采样 PA0，DMA2 Stream0 把结果搬进两块 buffer 交替，DMA 完成中断通知线程 A，线程 A 切换 buffer 并通知线程 B，线程 B 换算电压并打印。

当前进度：V2（malloc 双 buffer + `HAL_ADC_Start_DMA` + 完成/错误回调），已上板验证。

课程资料见 `课程资料/`，录制阅读记录见 `视频学习/`。
