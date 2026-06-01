# Unity 联机回合制卡牌对战 Demo

基于 **Unity + C# + .NET Socket API + Protobuf** 开发的双人联机回合制卡牌对战 Demo。项目包含 Unity 客户端与 C# 服务端，实现了登录、匹配、房间准备、进入战斗、回合同步、出牌、角色生成、攻击/防御、战斗结算、断线重连等基础联机流程。

项目采用 **服务端权威结算** 模式：客户端负责玩家操作提交与战斗表现播放，服务端负责战斗逻辑校验、状态切换、回合结算和同步广播。

------

## 项目预览

> 暂无图片与演示视频

- 登录与匹配
- 房间准备
- 回合制卡牌对战
- 角色生成、攻击、防御、属性变化
- 战斗事件顺序播放
- 断线重连与快照恢复

------

## 项目结构

```text
Project
├── Client / UnityClient
│   ├── Manager        # 客户端管理类，如战斗、角色、卡牌、玩家信息
│   ├── Service        # 网络服务封装，如 RoomService、BattleService
│   ├── Model          # 客户端战斗对象、UI 卡牌、角色表现
│   ├── UI             # 登录、房间、战斗界面
│   ├── Util           # 对象池、消息弹窗、战斗事件播放辅助
│   └── Resources      # 配置表、Prefab、ScriptableObject 资源
│
├── Server / GameServer
│   ├── Network        # TCP 连接、ClientSession、Socket 监听
│   ├── Manager        # 在线玩家、房间、战斗管理
│   ├── Model          # BattleRoom、BattleStateMachine、战斗状态
│   ├── Service        # GameService、NetService
│   └── Utils          # 战斗工具类、时间工具
│
├── Common
│   ├── Data           # CardData、CharacterData、BuffData
│   ├── Model          # Player、Card、Character、Room
│   └── Network        # MessageHandler、MessageDispatcher、ISession
│
└── Protocol
    └── message.proto  # Protobuf 协议定义
```

------

## 核心功能

### 1. 基础联机流程

- 玩家登录
- 双人匹配
- 房间创建与加入
- 准备状态同步
- 满员且双方准备后进入战斗
- 战斗结束后返回主界面

### 2. 回合制战斗流程

服务端将一场战斗拆分为以下状态：

```text
Loading      # 等待玩家加载或断线重连
SubmitBattle # 玩家提交出牌、攻击、防御等行动
Battle       # 服务端统一结算行动并广播战斗事件
RoundEnd     # 回合结算，判断胜负或进入下一回合
```

客户端根据服务端状态同步更新 UI，并在对应阶段限制玩家操作。

### 3. 卡牌与角色系统

当前 Demo 支持：

- 随机发牌
- 弃牌
- 使用角色卡生成角色
- 使用属性卡改变角色属性
- 角色攻击
- 角色防御
- 主角色保护规则
- 角色死亡与胜负结算

卡牌、角色和效果数据通过 JSON 配置表读取，客户端通过 ScriptableObject 管理 UI 图标、背景颜色和特效资源。

### 4. 服务端权威结算

客户端不会直接决定战斗结果，而是向服务端发送操作请求：

```text
客户端提交 BattleAction
        ↓
服务端校验操作是否合法
        ↓
服务端记录或立即执行操作
        ↓
战斗阶段统一排序并结算
        ↓
服务端生成 BattleEvent
        ↓
客户端按事件队列播放表现
```

这样可以避免客户端自行修改战斗结果，提高联机对战逻辑的一致性。

### 5. 战斗事件驱动表现

服务端结算后会广播战斗事件，例如：

- 抽牌事件
- 弃牌事件
- 角色生成事件
- 角色攻击事件
- 属性变化事件
- 角色死亡事件
- 行动冷却同步事件

客户端收到事件后，不直接写死流程，而是将事件加入队列，由 `BattleEventPlayer` 顺序播放攻击、受击、属性变化、死亡等表现。

### 6. 断线重连与快照恢复

项目实现了基于快照的断线重连机制。

玩家断线后重新登录并进入战斗时，服务端会同步当前战斗快照，包括：

- 当前房间 ID
- 当前回合数
- 当前战斗阶段
- 角色 ID、归属玩家、血量、攻击、防御、速度等属性
- 自己的手牌信息
- 敌方手牌数量
- 当前未结算的角色行动

客户端收到快照后，会清空本地旧战斗数据，并恢复当前战斗现场。

在战斗播放阶段，服务端还会为战斗事件分配 `eventBatchID` 和 `eventID`，用于记录客户端事件播放进度。玩家重连后，服务端可以根据播放进度补发尚未播放的战斗事件。

------

## 技术实现

### 网络通信

项目基于 **.NET Socket API** 封装 TCP 通信层，客户端与服务端使用统一的消息协议。

主要实现内容：

- TCP 长连接
- SocketAsyncEventArgs 异步收发
- 发送队列
- Protobuf 序列化与反序列化
- 4 字节长度包头封包
- 粘包与半包处理
- 消息分发器
- 请求响应回调
- 心跳检测
- 断线重连

### 协议设计

项目使用 Protobuf 定义通信协议，消息大体分为：

```text
RequestMessage
├── 登录请求
├── 匹配请求
├── 准备请求
├── 进入战斗请求
├── 战斗行动请求
├── 状态同步确认
├── 战斗事件播放完成请求
└── 心跳请求

ResponseMessage
├── 登录响应
├── 房间信息广播
├── 进入战斗通知
├── 回合开始广播
├── 战斗状态同步
├── 战斗事件广播
├── 战斗行动响应
├── 断线重连快照
├── 游戏结束响应
└── 心跳响应
```

### 服务端架构

服务端主要模块：

| 模块                     | 说明                               |
| ------------------------ | ---------------------------------- |
| `TcpSocketListener`      | 监听客户端连接，管理 ClientSession |
| `ClientSession`          | 管理单个客户端的收发、心跳和断开   |
| `MessageDispatcher`      | 根据消息类型分发到对应处理函数     |
| `OnlinePlayerManager`    | 管理在线玩家和重复登录             |
| `RoomManager`            | 管理匹配、房间、准备和退出         |
| `BattleManager`          | 管理战斗房间和战斗相关请求         |
| `BattleRoom`             | 一场战斗的核心上下文               |
| `BattleStateMachine`     | 战斗状态机                         |
| `BattleSyncSystem`       | 阶段切换同步和回合倒计时同步       |
| `BattleCardManager`      | 发牌、弃牌、手牌管理               |
| `BattleCharacterManager` | 角色生成、死亡、归属和冷却管理     |
| `BattleEffectManager`    | 属性变化与效果处理                 |
| `BattleMessageSystem`    | 战斗消息广播和单点发送             |

### 客户端架构

客户端主要模块：

| 模块                     | 说明                                   |
| ------------------------ | -------------------------------------- |
| `NetClient`              | TCP 连接、收发队列、断线重连           |
| `RoomService`            | 匹配、准备、退出房间请求               |
| `BattleService`          | 战斗协议封装和事件处理                 |
| `BattleManager`          | 当前回合、阶段和战斗事件队列           |
| `BattleCardManager`      | 手牌 UI、拖拽、出牌、弃牌              |
| `BattleCharacterManager` | 客户端角色对象管理                     |
| `BattleEventPlayer`      | 顺序播放服务端下发的战斗事件           |
| `BattleCharacter`        | 角色表现、状态图标、属性变化、死亡表现 |
| `UIBattleMain`           | 战斗主界面                             |
| `UIBattleCharacterPanel` | 角色攻击、防御、移除操作面板           |
| `UIBattleCharacterInfo`  | 角色悬停属性面板                       |
| `PoolManager`            | 游戏对象池                             |
| `VFXPlayer`              | 属性变化特效播放                       |

------

## 战斗流程

### 1. 进入战斗

```text
玩家匹配成功
    ↓
双方进入房间
    ↓
双方准备
    ↓
服务端创建 BattleRoom
    ↓
客户端切换到 BattleScene
    ↓
客户端发送 UserEnterBattleRequest
    ↓
服务端记录玩家加载完成
    ↓
双方加载完成后开始第一回合
```

### 2. 行动提交阶段

```text
服务端进入 SubmitBattle
    ↓
客户端进入行动阶段
    ↓
玩家可以出牌、弃牌、生成角色、选择攻击或防御
    ↓
服务端校验操作是否合法
    ↓
即时行为立即广播事件
    ↓
角色攻击/防御行为进入 pendingActions
```

### 3. 战斗结算阶段

```text
行动阶段时间结束
    ↓
服务端进入 Battle
    ↓
服务端根据行动类型和角色速度排序
    ↓
依次结算攻击、防御、治疗、死亡
    ↓
生成 BattleEvent 列表
    ↓
广播给客户端
    ↓
客户端按事件队列播放表现
```

### 4. 回合结束阶段

```text
客户端播放完成后回包
    ↓
服务端等待双方完成或超时
    ↓
进入 RoundEnd
    ↓
判断是否有玩家失败
    ↓
未结束则进入下一回合
```

------

## 运行方式

### 服务端

1. 打开服务端 C# 工程。
2. 确认配置文件位于服务端运行目录下，例如：

```text
Config/Card.json
Config/Character.json
Config/Effect.json
Info/Player.json
```

1. 启动服务端程序。
2. 默认监听地址：

```text
127.0.0.1:8080
```

### 客户端

1. 使用 Unity 打开客户端工程。
2. 确认客户端配置文件位于 Resources 目录下，例如：

```text
Resources/Config/Card.json
Resources/Config/Character.json
Resources/Config/Effect.json
Resources/SO/VisualContainer.asset
Resources/Prefab/Character
```

1. 先启动服务端，再运行 Unity 客户端。
2. 打开两个客户端实例，分别输入不同玩家 ID 登录。
3. 双方匹配并准备后进入战斗。

------

## 当前已实现

-  TCP 长连接通信
-  Protobuf 协议通信
-  粘包/半包处理
-  统一消息分发
-  登录流程
-  匹配与房间准备
-  回合状态同步
-  服务端战斗状态机
-  服务端权威结算
-  卡牌抽取与弃牌
-  角色生成
-  攻击与防御
-  属性变化
-  战斗事件队列播放
-  游戏胜负结算
-  心跳检测
-  断线重连
-  战斗快照恢复
-  战斗事件续播
-  对象池与属性变化 VFX

------

## 后续优化方向

- 完善异常包校验，增强服务端安全性
- 补充更多角色类型和技能机制
- 实现Buff系统，包括持续回合、移除和属性回滚
- 优化断线重连在战斗播放阶段的边界情况
- 将 IP、端口、回合时间、手牌上限等参数配置化
- 清理调试日志和临时代码
- 增加更完整的战斗演示视频和项目截图

------

## 技术栈

- Unity
- C#
- .NET Socket API
- SocketAsyncEventArgs
- Protobuf
- TCP
- JSON 配置表
- ScriptableObject
- 状态机
- 事件驱动
- 对象池

------

## 项目说明

本项目为个人学习与求职展示 Demo，主要目标是实现一个完整的 Unity 联机对战基础流程，并练习客户端与服务端之间的通信、同步、战斗结算和断线恢复机制。

项目当前更偏向小规模 Demo 验证，尚未针对大规模并发、账号安全、数据库持久化和商业化网络架构进行完整设计。