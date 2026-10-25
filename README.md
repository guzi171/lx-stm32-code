# 代码实现

构造 main → funcA → funcB 的调用链，在 funcB 中向非法地址写入触发 HardFault，结合异常栈帧与故障寄存器定位故障代码，已在 STM32F411 开发板上通过 J-Link 验证。完整工程见 F411_HardFault_Demo.zip。
