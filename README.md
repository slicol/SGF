# SGF 
the Simple Game Foundation

它主要由3个工程组成：

* SGFCore：与Unity无关的功能模块。
* SGFUnity：与Unity相关的功能模块。
* SGFDebuger：一个日志输出工具。

其余工程说明：
* ILRuntime：一个ILRuntime的第3方库。
* SGFServerLite：一个服务器的示例。
* SGFAppCommon/SGFAppILRScript/SGFAppDemo：一个客户端的综合演示示例，它包括与服务器通讯，热更新等等逻辑。

其余目录：
* Libs：以上工程编译好的Dll，如果不需要修改代码的话，你可以直接使用这些Dll。
* Apps：以上示例编译好的Exe，你可以尝试运行一下，可能在不同的机器上运行会有问题，你最好直接打开对应的示例工程来调试运行。

其它：
* SGF库发源于我的一个GAD课程项目：《贪吃蛇大作战》
* 其相关的视频链接为：
    * 第一季： http://gameinstitute.qq.com/lore/index/10007
    * 第二季： http://gameinstitute.qq.com/lore/index/10017

## 1.SGFCore
### Codec
* 收集或者实现一些常用编解码库，或者对一些编解码库进行易用性封装。
    * MiniJson，一个常用的轻量Json编解码库（Copyright (c) 2013 Calvin Rien）
    * PBSerializer，对Protobuf的封装，使之在一般场合下非常易用。
    * SGFEncoding，一些自己实现的编解码相关算法，比如:
        * XOR
        * CheckSum
        * BytesToHex,HexToBytes
        * 等等
    
### Common
* 将来可能放一些数据结构相关的类，或者易用性封装。
    * DictionarySafe，是对Dictionary做一个易用性封装。
    * MapList，是将List与Dictionary结合起来做一个易用性封装。
 
### Console
* 控制台相关的功能。
    * ConsoleInput：使控制台应用程序可以接受命令行输入。

### Event
* 事件系统。
* 它的思路与系统自带的Action类似，但是在性能与逻辑上比原生的Action更加可控，并且能够以日志的方式进行追踪和调试。
* 这在实际应用中非常有用。

### G3Lite
* 轻量级几何库。
* 考虑到很多时候需要脱离Unity进行开发，所以需要一套脱离Unity的几何类库。这里部分移植了geometry3Sharp里的一些基础类：
    - Vector2/Vector3/Vector4
    - Matrix3
    - Quaternion
    - MathUtil
    - IndexTypes

### IPCWork
* 进程通讯模块。
* 它基于UdpSocket实现，以RPC的方式进行调用。
    * 由于是基于Udp的简单实现，所以对单次通讯的数据大小有要求，不能超过当前系统的MTU。
    * 如果需要在进程间进行大数据通讯，可以使用Network模块，建立可靠UDP连接，或者TCP连接。当然也可以采用共享内存和管道方案，期待后续有人来完善。

### MathLite
* 计划实现一套轻量级的数学库。
* 目前只有随机数生成算法。

### Module
* 模块管理器。
* 当项目的规模足够大时，就需要将整个系统划分为若干个模块。这些模块需要一套系统进行管理。
* 它具备很好的扩展性，结合ILRuntime可以实现iOS版本中模块级别的热更新。
* 它实现了模块间的消息通讯，并以此来解耦。

### Network
* 网络模块。
* 它实现了与网络通讯相关的绝大部分功能。
    * Core，这里实现了一些数据、协议头、消息格式等的定义。还有一个轻量的RPC实现方案。
    * FSPLite，实现了一个轻量级的【帧同步】通讯方案的前台与后台模块。使用它，你可以很快搭建出一个帧同步运行环境，包括帧同步服务器和客户端。
    * General，实现了一个【通用】的网络通讯方案的前台与后台模块。使用它，你可以很容易实现游戏的状态同步，包括服务器和客户端。

### Server
* 服务器模块管理器。
* 在服务器开发中，当项目规范足够大时，也需要像客户端一样将整个系统划分为若干模块。这些模块需要进行管理。
* 目前该功能还比较简单，需要进一步完善。

### Time
* 与时间相关的类。
    - SGFTime，类似Unity里的Time类。需要在App启动时，进行初始化。

### Utils
* 一些工具类。
    - FileUtils，文件相关功能。
    - PathUtils，路径相关功能。
    - TimeUtils，时间相关功能。
    - 等等




## 2.SGFUnity
### DebugerGUI
* 一个可扩展的运行时调试和测试工具GUI。
* 可以很方便地在其上扩展自己的工具页签。

### ILR
* 热更新相关的功能模块。
    - DebugerILR，为热更模块提供日志输出功能。
    - ModuleILR，支持模块级别的热更。
    - UIILR，支持UI系统调用热更模块。
* 本热更方案可以实现在开发时，采用原生C#模式进行开发和单步调试。在发布时，无逢切换为热更模式。整个过程对于开发者是无感知的。

### UI
* UI系统。
* 这是一个基于UGUI的UI管理系统，封装了游戏开发中常用的几种UI元素。并且将这些元素分为2类，UIPage和UIPanel：页面。游戏的基础UI就是Page，它是全屏的。所有的游戏Panel都是显示在一个页面上的。
* 而UIPanel又可以分为以下几类：
    - UIWindow，游戏中的窗口。
    - UILoading，游戏中的Loading。
    - UIWidget，游戏中依附于窗口的一些小UI，或者会自动消失的小UI。
* 顺便封装了一些常用的通用UI面板和控件。
    - UIMsgBox，继承自UIWindow。
    - UIMsgTips，继承自UIWidget。
    - UISimpleLoading，继承自UILoading。
    - CtlList/CtlListItem
    - CtlProgressBar
    - 等等
    
### Utils
* 一些工具类。

## 3.SGFDebuger
* 一个日志工具类。
* 支持在非Unity环境和Unity环境下都能输出格式化日志。
* 并且支持日志保存为文件。 