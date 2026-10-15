# Git 命令助手

面向当前课程仓库工作方式的小型 Windows Git 工具：

- 左侧终端可以直接输入命令并按 Enter 执行；
- 右侧按钮分别执行初始化、查看状态、关联远程、暂存、提交和推送；
- 顶部工程目录是按钮操作的唯一目标；即使终端手动 `cd` 到别处，按钮命令也会先回到顶部选中的工程目录；
- 未初始化时仍可点击所有步骤：状态按钮给出中文提示，关联/添加/提交/推送会询问是否先初始化；也可直接使用“提交并推送”完成完整流程；
- 本地分支和远程分支默认同名，也允许分别设置；
- 推送前显示远程地址和分支映射并要求确认；
- “一键提交并推送”按顺序执行各步骤，任一步失败都会停止。

## 构建

在本目录执行：

```powershell
dotnet build .\GitCommandHelper.csproj
```

## 运行

```powershell
dotnet run --project .\GitCommandHelper.csproj
```

程序需要已安装 Git for Windows，并从标准安装目录寻找 `bash.exe`。

## 本地冒烟测试

测试只在系统临时目录创建本地仓库和裸远端，不连接网络：

```powershell
dotnet run --project .\GitCommandHelper.csproj -- --smoke-test
```

## 后台渲染界面预览

窗口在屏幕外创建并保存 PNG，不抢占当前前台窗口：

```powershell
dotnet run --project .\GitCommandHelper.csproj -- --render-preview .\artifacts\ui-preview.png
```
