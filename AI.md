# ZombieAutoClicker - 项目智能文档

## 项目概述

**ZombieAutoClicker** 是一个针对游戏《向僵尸开炮》的Windows桌面自动化辅助工具。该项目使用C# .NET 8.0开发，采用模块化架构设计，结合计算机视觉（OCR）和图像识别技术，实现游戏界面的自动识别与操作。

## 核心功能

### 1. 游戏自动化挂机
- **元素试炼模式**：自动完成游戏中的元素试炼关卡
- **多模式支持**：支持雾境迷宫、寰球试炼、主线推图等模式（开发中）
- **智能状态机**：采用严格的两阶段状态机控制流程

### 2. 视觉识别系统
- **PaddleOCR集成**：基于深度学习的文字识别，高准确率
- **OpenCV图像模板匹配**：支持PNG/JPG图片模板识别
- **混合识别引擎**：文字与图片混合识别，根据后缀自动选择识别方式

### 3. 用户界面
- **主控制面板**：模式选择、开始/停止控制、实时日志显示
- **悬浮窗显示**：实时显示OCR识别结果和边界框
- **自适应界面**：支持窗口大小调整，控件自动布局

## 技术架构

### 编程语言与框架
- **语言**：C# (.NET 8.0)
- **框架**：Windows Forms
- **目标平台**：Windows 10/11 (x64/x86)
- **架构模式**：模块化设计，接口驱动，依赖注入

### 核心依赖
- **PaddleOCRSharp** (v6.1.0)：深度学习OCR引擎
- **OpenCvSharp4** (v4.13.0)：计算机视觉处理
- **Windows API**：窗口捕获和鼠标控制

### 项目结构（模块化重构版）
```
ZombieAutoClicker/
├── ZombieAutoClicker.sln              # Visual Studio解决方案文件
├── ZombieAutoClicker/                 # 主项目目录
│   ├── Program.cs                     # 应用程序入口点
│   ├── MainForm.cs                    # 主窗体逻辑（已适配新架构）
│   ├── MainForm.Designer.cs           # 主窗体设计器代码
│   ├── OverlayForm.cs                 # 悬浮窗逻辑（已适配新架构）
│   ├── OverlayForm.Designer.cs        # 悬浮窗设计器代码
│   ├── ZombieAutoClicker.csproj       # 项目配置文件
│   ├── Core/                          # 核心抽象层
│   │   ├── Interfaces/                # 接口定义
│   │   │   ├── IVisionService.cs      # 视觉识别服务接口
│   │   │   ├── IWindowService.cs      # 窗口控制服务接口
│   │   │   └── IGameBotController.cs  # 游戏机器人控制器接口
│   │   └── ServiceFactory.cs          # 服务工厂（依赖注入）
│   ├── Modules/                       # 模块实现层
│   │   ├── Vision/                    # 视觉识别模块
│   │   │   └── CompositeVisionService.cs  # 复合视觉服务（OCR+图像匹配）
│   │   └── WindowControl/             # 窗口控制模块
│   │       └── WindowsWindowService.cs    # Windows窗口服务
│   ├── Controllers/                   # 控制器层
│   │   └── GameBotController.cs       # 游戏机器人主控制器（使用接口依赖）
│   └── Interop/                       # 系统交互层
│       └── NativeMethods.cs           # Windows API封装（供内部使用）
├── bin/                               # 编译输出目录
│   └── Debug/net8.0-windows10.0.19041.0/
│       ├── inference/                 # OCR模型文件
│       └── runtimes/                  # 运行时依赖
└── obj/                               # 编译中间文件
```

## 核心组件详解

### 1. 接口层设计（Core/Interfaces/）

#### IVisionService - 视觉识别服务接口
**功能**：
- 统一OCR文字识别和图像模板匹配的接口
- 根据目标字符串后缀自动选择识别方式（.png/.jpg为图像，其他为文字）
- 提供OCR识别结果事件广播

#### IWindowService - 窗口控制服务接口
**功能**：
- 封装窗口查找、截图和鼠标控制功能
- 提供窗口相对坐标到屏幕绝对坐标的转换
- 统一的窗口操作抽象

#### IGameBotController - 游戏机器人控制器接口
**功能**：
- 定义游戏自动化控制的标准接口
- 提供启动、停止和运行状态查询

### 2. 模块实现层（Modules/）

#### CompositeVisionService - 复合视觉服务
**位置**: `Modules/Vision/CompositeVisionService.cs`

**功能**：
- 整合PaddleOCR文字识别和OpenCV图像模板匹配
- 线程安全的OCR引擎管理
- 智能识别类型判断（根据文件扩展名）

**技术特性**：
- 静态OCR引擎实例，避免重复加载模型
- 支持中英文混合识别
- 图像模板匹配阈值可配置（默认0.8）
- 类型安全：明确区分System.Drawing和OpenCvSharp类型

#### WindowsWindowService - Windows窗口服务
**位置**: `Modules/WindowControl/WindowsWindowService.cs`

**功能**：
- 封装Windows API调用
- 窗口查找和截图功能
- 鼠标点击模拟
- 显式接口实现避免Windows API方法名冲突

### 3. 服务工厂（Core/ServiceFactory.cs）

**功能**：
- 统一的服务实例管理
- 简单的依赖注入容器
- 服务生命周期管理

### 4. GameBotController - 游戏机器人控制器（重构版）
**位置**: `Controllers/GameBotController.cs`

**功能**：
- 实现严格的两阶段状态机控制
- 阶段1：进图准备阶段（顺序执行图片模板点击）
- 阶段2：战斗循环阶段（优先级循环执行文字识别点击）
- 智能重置机制：检测到返回主界面时自动重置状态

**重构改进**：
- 通过构造函数注入依赖（IVisionService, IWindowService）
- 实现IGameBotController接口
- 移除对具体实现的直接依赖

### 5. NativeMethods - 系统交互层
**位置**: `Interop/NativeMethods.cs`

**功能**：
- Windows API原生封装
- 被WindowsWindowService内部使用
- 保持向后兼容性

### 6. MainForm - 主控制界面（已适配）
**位置**: `MainForm.cs`

**功能**：
- 使用ServiceFactory创建服务实例
- 订阅OCR识别事件，更新悬浮窗显示
- 提供用户交互界面

### 7. OverlayForm - 悬浮窗（已适配）
**位置**: `OverlayForm.cs`

**功能**：
- 全屏透明悬浮窗，显示OCR识别结果
- 实时绘制文字边界框
- 显示识别到的文本内容

## 工作流程

### 自动化执行流程
1. **初始化**：启动程序，ServiceFactory创建服务实例
2. **界面加载**：显示主界面和悬浮窗，订阅OCR事件
3. **模式选择**：用户选择挂机模式（如"元素试炼"）
4. **开始挂机**：
   - GameBotController通过IWindowService查找游戏窗口
   - 通过IVisionService进行屏幕识别
   - 阶段1：按顺序点击进图按钮（图片模板识别）
   - 阶段2：循环检测战斗界面文字并点击（OCR文字识别）
   - 智能重置：检测到返回主界面时自动重新开始
5. **停止挂机**：用户点击停止按钮，结束自动化流程

### 识别流程
1. **目标判断**：根据目标字符串后缀判断识别类型
   - `.png`/`.jpg`/`.jpeg`：图像模板匹配（使用OpenCV）
   - 其他：OCR文字识别（使用PaddleOCR）
2. **图像处理**：截取游戏窗口画面
3. **识别执行**：
   - 图片：使用OpenCV进行模板匹配
   - 文字：使用PaddleOCR进行文字识别
4. **坐标计算**：计算识别目标的中心点坐标
5. **点击执行**：转换为屏幕绝对坐标并模拟点击

## 配置与部署

### 环境要求
- Windows 10/11 操作系统
- .NET 8.0 Runtime
- 游戏《向僵尸开炮》窗口需要可见

### 资源文件
- **图片模板**：需要放置在`assets/`目录下
  - `assets/btn_start_element.png`：开始按钮
  - `assets/GameStart.png`：游戏开始按钮
- **OCR模型**：自动从`inference/`目录加载

### 编译与运行
1. **编译**：使用Visual Studio或`dotnet build`命令
2. **运行**：启动`ZombieAutoClicker.exe`
3. **配置**：确保游戏窗口标题为"向僵尸开炮"

## 开发约束与注意事项

### 技术约束
1. **模块化架构**：必须通过接口访问服务，禁止直接实例化具体实现
2. **窗口依赖**：必须找到标题为"向僵尸开炮"的游戏窗口
3. **图片路径**：图片模板必须放在exe同级目录的assets文件夹
4. **分辨率适配**：目前针对特定游戏分辨率优化，可能需要调整识别区域

### 安全与合规
1. **仅限个人使用**：自动化工具可能违反游戏服务条款
2. **本地处理**：所有识别在本地完成，无网络传输
3. **无恶意行为**：仅模拟鼠标点击，不修改游戏内存

### 扩展性设计
1. **接口扩展**：通过实现IVisionService/IWindowService接口添加新功能
2. **模块替换**：可轻松替换OCR引擎或窗口控制实现
3. **配置扩展**：可通过修改ServiceFactory调整服务实现

## 故障排查

### 常见问题
1. **找不到游戏窗口**：确保游戏已启动且窗口标题正确
2. **识别失败**：检查图片模板路径和文件是否存在
3. **点击位置不准**：可能需要调整游戏分辨率或识别参数
4. **OCR识别慢**：首次加载模型需要时间，后续识别会加快

### 调试信息
- 查看主界面日志框获取实时运行状态
- 悬浮窗显示当前识别到的文字和边界框
- 可通过修改日志级别获取更详细的调试信息

## 架构优势

### 1. 模块化设计
- OCR模块与窗口控制模块完全隔离
- 接口驱动，易于单元测试
- 可替换的实现，便于技术升级

### 2. 可维护性
- 清晰的职责分离
- 统一的错误处理机制
- 完善的日志记录

### 3. 可扩展性
- 新增识别算法只需实现IVisionService接口
- 支持多种窗口控制方式
- 易于添加新的游戏模式

### 4. 类型安全
- 明确区分System.Drawing和OpenCvSharp类型
- 避免命名空间冲突
- 编译时类型检查

## 未来改进方向

### 功能增强
1. **更多游戏模式**：完善雾境迷宫、寰球试炼等模式
2. **配置界面**：添加识别参数配置界面
3. **脚本系统**：支持用户自定义自动化脚本
4. **多语言支持**：扩展OCR支持更多语言

### 技术优化
1. **识别精度提升**：优化图像预处理和识别算法
2. **性能优化**：减少内存占用，提高识别速度
3. **错误恢复**：增强异常处理和自动恢复机制
4. **测试框架**：添加完整的单元测试和集成测试

### 架构演进
1. **完整DI容器**：引入Microsoft.Extensions.DependencyInjection
2. **配置外部化**：将硬编码参数迁移到配置文件
3. **插件系统**：支持动态加载功能模块
4. **跨平台支持**：探索Linux/macOS兼容性

---

*本文档最后更新：2026-03-27*
*项目版本：v2.0（模块化重构版）*
*文档用途：为AI Agent提供项目上下文，支持智能代码分析和任务执行*