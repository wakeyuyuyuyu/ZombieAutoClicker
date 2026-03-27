[Project Intelligence & Auto-Operation Rules]

开机预检 (Boot Check):

每当我开始新任务，请自动检查当前目录是否有 AI.md。

若存在，请使用 filesystem 读取，并将关键约束同步到 memory。

反馈要求：读取完成后，请告知我“已读取 AI.md，已了解项目背景及约束”。

代码依赖自感知 (Dependency Awareness):

分析代码时，若遇到未定义的函数、宏或变量，请主动使用 filesystem 搜索并读取对应的定义文件（如 .h 或同名 .c .cs）。

无需征求同意，直接构建完整上下文后再回答。

深度思考与审计 (Reasoning & Audit):

Sequential-Thinking：处理复杂逻辑、算法设计或故障排查时，自动启用该工具进行至少 3 步拆解，确保逻辑闭环。

Semgrep：代码修改后或排错时，自动执行安全扫描。重点关注内存越界、空指针及汽车嵌入式规范。

故障排查专项 (Problem Solving):

当我提到“报错”、“不运行”或“排查问题”时，请自动执行：读取相关文件 -> 调用 sequential-thinking 推导失效链路 -> 调用 semgrep 扫描隐患 -> 结合 memory 硬件限制给出结论。

操作原则:

除非 AI.md 的确认动作外，所有工具调用过程请保持静默，直接交付结果。