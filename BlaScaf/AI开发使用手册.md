# BlaScaf AI 开发使用手册

## 1. 文档目的

本文档面向“拿不到 `BlaScaf` 框架源码，但需要基于该框架开发新系统”的 AI 或开发者。

目标不是解释每一行实现，而是说明：

1. `BlaScaf` 是什么。
2. 宿主系统应该如何接入它。
3. 开发新页面、新菜单、新权限、新用户体系时必须满足哪些约定。
4. 哪些配置是必填的，哪些扩展点是可选的。
5. AI 在没有源码时，如何根据本手册推导出一个可运行的新系统骨架。

---

## 2. 框架定位

`BlaScaf` 是一个基于 `.NET 8 + Blazor Server + AntDesign Blazor` 的后台脚手架库，项目输出类型是 `Library`，不是独立可执行程序。

这意味着：

- `BlaScaf` 本身提供的是“后台系统框架能力”。
- 真正运行的系统应是一个单独的宿主 Web 项目。
- 宿主项目通过引用 `BlaScaf.dll` 或项目引用的方式接入框架。
- 业务页面、业务实体、数据库连接、角色、菜单、验证码、头部扩展等，原则上都在宿主项目中完成。

一句话理解：

> `BlaScaf` 负责后台系统通用底座，宿主项目负责具体业务系统实现。

---

## 3. 技术栈与内置能力

### 3.1 技术栈

- `.NET 8`
- `Blazor Server`
- `AntDesign Blazor`
- `FreeSql`（框架内置了 FreeSql 的示例接法，但并不强制只能使用 FreeSql）

### 3.2 内置能力

框架默认包含以下能力：

- Cookie 登录认证
- 基于角色的菜单显示
- 基于路由的页面权限控制
- 用户管理页面
- 系统日志页面
- 操作日志页面
- 数据库管理页面（基于 `IFreeSql`）
- 修改密码弹窗
- 顶部扩展插槽
- 可选验证码扩展
- 页面标题自动切换
- KeepAlive 保活与断线重连容忍

---

## 4. 总体架构

在“无源码开发”场景下，可以把框架理解成下面 4 层：

### 4.1 框架层

由 `BlaScaf` 提供：

- `Startup.AddBsService(IServiceCollection)`
- `Startup.UseBsService(WebApplication)`
- `BsConfig`
- `BsUser / BsMenuItem / BsOptLog / BsSysLog / QueryRsp<T>`
- 框架内置页面：`/login`、`/users`、`/optlogs`、`/syslogs`、`/dbadmin`
- 主布局、菜单、认证状态管理、登录 API、退出 API、拒绝访问页

### 4.2 宿主层

由你的新系统项目提供：

- `Program.cs`
- 数据库连接
- 角色定义
- 菜单定义
- 用户初始化与用户缓存加载
- 业务页面
- 业务实体
- 日志持久化实现
- 验证码组件 / 顶部扩展组件 / 用户权限弹窗等可选扩展

### 4.3 运行时配置层

宿主通过给 `BsConfig` 赋值，将系统的全局行为注入框架。例如：

- 系统名
- Cookie 超时时间
- 用户角色列表
- 菜单树
- 用户缓存
- 新增/编辑用户回调
- 记录登录信息回调
- 查询系统日志/操作日志回调

### 4.4 页面层

业务页面只要被宿主程序集编译进去，并且：

- 路由可被框架发现
- 当前登录角色有对应菜单权限

就可以接入统一布局、统一认证和统一菜单体系。

---

## 5. 宿主项目的最小接入方式

最标准的做法就是参考 `DemoApp` 的模式，创建一个 Web 宿主项目。

### 5.1 宿主项目应具备

1. 引用 `BlaScaf`
2. 配置数据库与 ORM
3. 在 `Program.cs` 中完成 `BsConfig` 初始化
4. 调用：

```csharp
builder.Services.AddBsService();
app.UseBsService();
```

### 5.2 最小项目文件示例

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\BlaScaf\BlaScaf.csproj" />
    <PackageReference Include="FreeSql.Provider.Sqlite" Version="3.5.206" />
  </ItemGroup>
</Project>
```

### 5.3 最小 `Program.cs` 模板

```csharp
using BlaScaf;
using FreeSql;

var adminRole = "管理员";
var userRole = "普通用户";

BsConfig.AppName = "我的新系统";
BsConfig.CookieTimeOutMinutes = 30;
BsConfig.ChangePwdDays = 90;

BsConfig.Roles = new List<string> { adminRole, userRole };

BsConfig.MenuItems = new List<BsMenuItem>
{
    new BsMenuItem
    {
        Key = "home",
        Title = "首页",
        Icon = "home",
        RouterLink = "/",
        Roles = new List<string> { adminRole, userRole }
    },
    new BsMenuItem
    {
        Key = "users",
        Title = "用户管理",
        Icon = "user",
        RouterLink = "/users",
        Roles = new List<string> { adminRole }
    }
};

var builder = WebApplication.CreateBuilder(args);

var fsql = new FreeSqlBuilder()
    .UseConnectionString(DataType.Sqlite, "Data Source=myapp.db")
    .UseAutoSyncStructure(true)
    .Build();

Startup.InitFreeSqlActionFunc(fsql);

BsConfig.Users = fsql.Select<BsUser>().ToList();
BsConfig.AddLogin = user =>
{
    fsql.Update<BsUser>().SetSource(user).ExecuteAffrows();
};

Startup.CheckBsConfig();

builder.Services.AddSingleton<IFreeSql>(fsql);
builder.Services.AddBsService();

var app = builder.Build();
app.UseBsService();
app.Run();
```

---

## 6. AI 必须理解的核心契约：`BsConfig`

`BsConfig` 是整个框架最重要的配置入口。  
可以把它理解为“宿主系统对框架的声明式注入”。

### 6.1 必填项

下列配置在真实项目中应视为必填：

| 配置项 | 作用 | 说明 |
| --- | --- | --- |
| `AppName` | 系统名称 | 显示在登录页、浏览器标题等 |
| `CookieTimeOutMinutes` | 登录超时分钟数 | 决定 Cookie 过期时间和前端保活节奏 |
| `Roles` | 角色列表 | 用户管理页角色选择、菜单权限判断都依赖它 |
| `MenuItems` | 菜单树 | 页面导航、权限校验、标题显示都依赖它 |
| `Users` | 用户缓存 | 登录、认证重校验、当前用户解析依赖它 |
| `AddOrUpdateUser` | 新增/更新用户 | 必须同时更新数据库和 `BsConfig.Users` |
| `AddOptLog` | 写入操作日志 | 用户管理、数据库管理等功能会调用 |
| `AddSysLog` | 写入系统日志 | 登录成功/失败等会调用 |
| `GetOptLogs` | 查询操作日志 | `/optlogs` 页面依赖 |
| `GetSysLogs` | 查询系统日志 | `/syslogs` 页面依赖 |
| `AddLogin` | 登录成功后持久化用户状态 | 虽然校验方法未强制检查，但登录 API 实际会直接调用，因此也应视为必填 |

### 6.2 常用可选项

| 配置项 | 作用 |
| --- | --- |
| `UseSessionCookie` | 是否使用会话 Cookie |
| `ChangePwdDays` | 多少天后强制修改密码，`<= 0` 可视为不启用 |
| `HeadInjectRawHtmls` | 向 `<head>` 注入原始 HTML，例如外部脚本 |
| `HeaderFragments` | 顶部导航栏右侧扩展内容 |
| `CaptchaFragment` | 登录页验证码组件 |
| `CaptchaRoles` | 哪些角色登录时需要验证码 |
| `UserAuthFragment` | 用户管理页“权限”扩展弹窗 |
| `AnonymousPages` | 不需要登录的页面类型集合 |
| `RouterLinkPages` | 不显示在菜单里，但允许访问的路由 |
| `DbAdminEntityTypes` | 数据库管理页允许映射的实体类型集合 |
| `SetBrowserTitle` | 自定义浏览器标题生成规则 |

---

## 7. 菜单、路由、页面权限的真实工作方式

这是开发新页面时最容易踩坑的部分。

### 7.1 菜单模型

`BsMenuItem` 结构如下：

- `Title`：菜单标题
- `Key`：菜单唯一键
- `Icon`：AntDesign 图标名
- `RouterLink`：路由，例如 `/orders`
- `Roles`：允许访问的角色列表
- `Children`：子菜单

### 7.2 菜单不仅决定显示，还决定页面能否访问

框架不是只用菜单做导航。

主布局会在路由变化时：

1. 从 `BsConfig.MenuItems` 中按当前地址查找菜单。
2. 如果没找到，再去 `BsConfig.RouterLinkPages` 中找。
3. 如果仍然找不到，或者当前用户角色不在 `Roles` 中，直接跳转到 `/api/denied`。

这意味着：

> 新页面即使写好了路由，只要没有登记到 `MenuItems` 或 `RouterLinkPages`，依然会被判定为无权限。

### 7.3 开发新页面时必须同步做的事

新增一个业务页面时，至少同时完成：

1. 创建 `.razor` 页面并声明 `@page`
2. 给页面加 `@attribute [Authorize]`（如果需要登录）
3. 在 `BsConfig.MenuItems` 中增加菜单，或在 `RouterLinkPages` 中登记隐藏路由
4. 给该菜单配置可访问角色

### 7.4 宿主程序集页面会被自动发现

框架在路由中会额外加载入口程序集，因此宿主项目里定义的 Razor 页面会被一起扫描和注册。  
也就是说，业务页面通常应该写在宿主项目，而不是改框架库本身。

---

## 8. 认证与登录机制

### 8.1 认证方式

框架使用 ASP.NET Core Cookie 认证，登录成功后会写入 Claims：

- 用户名
- 用户全名
- 用户 ID
- 角色
- Token
- IP
- User-Agent

### 8.2 登录流程

登录分为两段：

1. 登录页前端先将密码做 `MD5`，再用一次性随机 key 进行 `DES` 加密。
2. `/api/login` 在服务端解密后，与 `BsUser.Password` 比较。

### 8.3 用户缓存是认证的权威数据源

认证状态重校验不是直接查数据库，而是查 `BsConfig.Users`。

重校验逻辑重点依赖：

- `UserId`
- `Token`
- `EndTime`

因此以下动作都必须同步更新 `BsConfig.Users`：

- 新增用户
- 编辑用户
- 修改密码
- 登录时更新 Token
- 用户禁用/过期变更

如果数据库变了，但 `BsConfig.Users` 没更新，会出现：

- 刚改完数据，页面权限仍旧不对
- 登录态异常失效
- Token 校验失败

### 8.4 `AddLogin` 的职责

登录成功后框架会更新用户对象的：

- `Token`
- `LastLogin`
- `LastIP`

然后调用 `BsConfig.AddLogin(user)`。

宿主项目必须在这里把这些变更持久化到数据库。  
否则页面刷新或重新校验时，Token 很可能失效。

---

## 9. 用户模型约定

框架内置用户实体是 `BsUser`，包含但不限于：

- `UserId`
- `UserName`
- `FullName`
- `Password`
- `Role`
- `Enable`
- `LastChangePwd`
- `Token`
- `LastLogin`
- `EndTime`
- `ExtField1 ~ ExtField5`
- `ExtJson`

### 9.1 用户名规则

内置校验要求：

- 不能为空
- 长度 2 到 24
- 仅允许字母和数字

### 9.2 密码规则

用户管理与修改密码默认要求：

- 长度至少 8 位
- 必须包含数字
- 必须包含小写字母
- 必须包含大写字母
- 长度不能大于 32

### 9.3 推荐做法

如果新系统有更复杂的用户体系，推荐：

- 保留 `BsUser` 作为后台登录账户模型
- 业务侧再扩展自己的用户资料表
- 用 `ExtJson` 或独立表存放个性化字段

---

## 10. 日志体系接入方式

框架内置两类日志：

### 10.1 系统日志 `BsSysLog`

适合记录：

- 登录成功
- 登录失败
- 异常事件
- 系统状态变化

页面：`/syslogs`

### 10.2 操作日志 `BsOptLog`

适合记录：

- 添加用户
- 编辑用户
- 数据导入
- 审核动作
- 数据库管理页的增删改查与 SQL 执行

页面：`/optlogs`

### 10.3 宿主必须提供的日志能力

你需要实现：

- `BsConfig.AddSysLog`
- `BsConfig.AddOptLog`
- `BsConfig.GetSysLogs`
- `BsConfig.GetOptLogs`

如果用 FreeSql，可以直接复用：

```csharp
Startup.InitFreeSqlActionFunc(fsql);
```

该方法会自动帮你接好：

- 用户新增/更新
- 系统日志写入
- 操作日志写入
- 两类日志分页查询

---

## 11. 页面开发规范

### 11.0 框架内置页面清单

框架已经提供以下路由页面：

- `/login`
- `/users`
- `/optlogs`
- `/syslogs`
- `/dbadmin`

注意：

- 这些页面虽然由框架提供，但除 `/login` 外，仍然需要宿主在 `MenuItems` 或 `RouterLinkPages` 中登记后才能正常访问。
- 如果宿主没有给某个内置页面配置菜单或隐藏路由，该页面同样会被权限系统拦截。

### 11.1 新业务页面的推荐写法

```razor
@page "/orders"
@attribute [Authorize]

<PageHeader Title="订单管理" />

<div>这里放你的业务内容</div>
```

### 11.2 对应菜单配置

```csharp
BsConfig.MenuItems.Add(new BsMenuItem
{
    Key = "orders",
    Title = "订单管理",
    Icon = "table",
    RouterLink = "/orders",
    Roles = new List<string> { "管理员", "运营" }
});
```

### 11.3 隐藏路由页面

如果页面不希望出现在左侧菜单，但仍需要可访问，例如详情页：

```csharp
BsConfig.RouterLinkPages.Add(new BsMenuItem
{
    Key = "order-detail",
    Title = "订单详情",
    RouterLink = "/orders/detail",
    Roles = new List<string> { "管理员", "运营" }
});
```

### 11.4 带参数页面的建议

框架的权限匹配支持：

- 完整等于某路由
- 或当前地址以 `菜单路由 + "/"` 开头

例如：

- 菜单配置 `/orders`
- 页面访问 `/orders/123`

这种情况下仍会被视为命中该菜单。

因此建议列表页和详情页使用同一路由前缀。

### 11.5 内置用户管理页的特殊规则

框架内置 `/users` 页面有一条硬编码规则：

- 当被编辑用户与当前登录用户属于同一角色时
- 只有用户名为 `admin` 的账户，才允许修改其他同角色管理员账号

因此如果你的系统不希望依赖 `admin` 这个特殊用户名，建议：

- 要么保留一个超级管理员账号名为 `admin`
- 要么自行替换/重写用户管理页逻辑
- 要么在宿主业务中避免使用该内置规则

---

## 12. 顶部扩展、验证码、权限弹窗扩展

框架提供三个很实用的宿主扩展点。

### 12.1 头部扩展 `HeaderFragments`

可用于放：

- 环境标签
- 当前时间
- 系统公告
- 快捷操作按钮

示例：

```csharp
RenderFragment fragment = builder =>
{
    builder.OpenComponent<MyHeaderBadge>(0);
    builder.CloseComponent();
};

BsConfig.HeaderFragments.Add(fragment);
```

### 12.2 验证码扩展 `CaptchaFragment + CaptchaRoles`

只给某些角色启用验证码时：

```csharp
BsConfig.CaptchaRoles = new List<string> { "审计员" };
BsConfig.CaptchaFragment = () => builder =>
{
    builder.OpenComponent<MyCaptchaComponent>(0);
    builder.CloseComponent();
};
```

### 12.3 用户权限扩展 `UserAuthFragment`

用于在 `/users` 页面点击“权限”时弹出自定义界面，例如：

- 菜单授权
- 数据范围授权
- 按钮权限授权

签名形式是：

```csharp
Func<BsUser, Func<Task>, RenderFragment>
```

含义：

- 第一个参数：当前编辑的用户
- 第二个参数：关闭弹窗时要调用的回调
- 返回值：一个可渲染的 Blazor 片段

---

## 13. 数据库管理页接入规则

内置页面 `/dbadmin` 依赖 `IFreeSql`。

### 13.1 它能做什么

- 读取真实数据库表结构
- 展示表字段
- 展示实体与表字段映射关系
- 查看表数据
- 新增/编辑/删除记录
- 建表、删表、加字段、删字段、改字段名
- 执行原生 SQL

### 13.2 启用前提

宿主必须注册：

```csharp
builder.Services.AddSingleton<IFreeSql>(fsql);
```

框架中的 `BsDbAdminService` 会通过依赖注入拿到 `IFreeSql`。

### 13.3 实体范围控制

如果你不希望数据库管理页扫描所有实体，可指定：

```csharp
BsConfig.DbAdminEntityTypes = new List<Type>
{
    typeof(BsUser),
    typeof(BsOptLog),
    typeof(BsSysLog),
    typeof(MyOrder)
};
```

### 13.4 适用场景

适合：

- 内部运维
- 测试环境排障
- 管理后台辅助核查

不建议直接暴露给普通业务角色。

---

## 14. 静态资源与前端注入规则

### 14.1 Logo 与图片

主布局默认会读取：

- `/images/logo_fold.png`
- `/images/logo_unfold.png`

因此宿主项目建议在 `wwwroot/images/` 下提供这两个文件。

### 14.2 额外脚本

如果需要额外前端脚本，可放到宿主的 `wwwroot` 下，再通过：

```csharp
BsConfig.HeadInjectRawHtmls.Add("<script src='test.js'></script>");
```

注入到页面头部。

### 14.3 浏览器标题

默认规则是：

```text
{AppName} - {当前导航标题}
```

也可以自定义：

```csharp
BsConfig.SetBrowserTitle = navTitle => $"我的系统 | {navTitle}";
```

---

## 15. AI 基于该框架开发新系统的标准流程

如果 AI 只能拿到本手册，建议严格按下面流程工作。

### 15.1 第一步：收集宿主系统的必要输入

至少确认：

1. 系统名称
2. 角色列表
3. 数据库类型
4. 登录账号来源
5. 需要哪些基础菜单
6. 哪些页面需要隐藏路由
7. 是否启用验证码
8. 是否启用数据库管理页
9. 是否需要用户权限弹窗
10. 是否使用 FreeSql，还是自己实现 `BsConfig` 委托

### 15.2 第二步：生成宿主项目骨架

创建：

- `Program.cs`
- `Pages/`
- `Shared/`
- `wwwroot/images/`
- ORM 与实体

### 15.3 第三步：初始化 `BsConfig`

至少完成：

- `AppName`
- `CookieTimeOutMinutes`
- `Roles`
- `MenuItems`
- `Users`
- `AddOrUpdateUser`
- `AddLogin`
- `AddOptLog`
- `AddSysLog`
- `GetOptLogs`
- `GetSysLogs`

### 15.4 第四步：开发业务页面

每加一个页面，检查以下清单：

- 是否声明 `@page`
- 是否需要 `[Authorize]`
- 是否有菜单或隐藏路由登记
- 是否配置了 `Roles`
- 是否需要写操作日志

### 15.5 第五步：验证登录链路

至少验证：

- 登录是否成功
- 刷新页面后登录态是否保留
- 左侧菜单是否按角色显示
- 越权访问是否跳转到拒绝页
- 用户禁用/过期后是否能被踢出

### 15.6 第六步：验证配置一致性

重点检查：

- `BsConfig.Users` 是否与数据库同步
- `AddLogin` 是否真的持久化了 Token
- 页面路由是否都能在菜单或隐藏路由中找到
- 日志委托是否可用

---

## 16. 推荐的开发策略

### 16.1 推荐：宿主项目承载业务

最佳实践是：

- `BlaScaf` 不改或少改
- 新系统逻辑主要写在宿主项目
- 宿主通过 `BsConfig` 注入行为

这样更利于：

- 多系统复用同一框架
- 降低框架升级成本
- 保持业务代码边界清晰

### 16.2 推荐：统一把权限收敛到角色 + 菜单

在框架现有设计下，最稳定的方式是：

- 页面级权限用角色控制
- 菜单级权限与页面权限保持一致
- 更细粒度的业务授权放到 `UserAuthFragment` 或业务服务内部

### 16.3 推荐：用户信息用缓存 + 数据库双写

任何会影响登录态或权限的数据修改，都要做到：

1. 更新数据库
2. 更新 `BsConfig.Users`

---

## 17. 生产使用前建议重点复核的事项

虽然框架已经具备完整后台骨架，但在生产系统中，建议宿主团队重点复核以下能力是否满足要求：

### 17.1 密码与安全策略

当前框架采用：

- 前端 `MD5`
- 再做一次 `DES` 传输加密
- 服务端比对存储的 MD5

这更接近“框架内置约定”，不一定满足所有生产安全规范。  
如果项目对安全要求较高，建议评估是否需要：

- 更强的密码哈希算法
- 更严格的登录风控
- 更完整的验证码机制

### 17.2 用户缓存同步机制

`BsConfig.Users` 是关键运行时缓存。  
如果系统支持后台改人、批量禁用、组织同步等能力，必须设计可靠的缓存刷新策略。

### 17.3 日志完整性

框架只提供日志入口，不负责你业务侧所有日志定义。  
关键业务动作应主动补充 `BsOptLog` 或独立审计日志。

### 17.4 数据库管理页权限

`/dbadmin` 功能很强，建议只开放给极少数管理员。

---

## 18. 一个新系统最少需要落地哪些内容

如果要基于 `BlaScaf` 快速生成一个新系统，最少需要有：

1. 一个宿主 Web 项目
2. 一个数据库连接
3. 一张用户表或沿用 `BsUser`
4. 角色列表
5. 菜单配置
6. 日志落库逻辑
7. 首页页面
8. 若干业务页面
9. `Program.cs` 中的 `BsConfig` 初始化
10. `builder.Services.AddBsService()` 与 `app.UseBsService()`

---

## 19. AI 可直接复用的开发模板思路

当 AI 接到“基于 BlaScaf 开发一个 XX 系统”的任务时，可以直接按下面套路输出：

### 19.1 先生成角色

例如：

- 管理员
- 审核员
- 业务员

### 19.2 再生成菜单

例如：

- 首页
- 用户管理
- 订单管理
- 报表中心
- 操作日志
- 系统日志

### 19.3 再生成实体与页面

例如：

- `Order` 实体
- `/orders` 列表页
- `/orders/{id}` 详情页

### 19.4 最后接入框架

- 配置 `BsConfig`
- 注入 ORM
- 初始化用户
- 接好日志委托
- 启动并测试

---

## 20. 结论

`BlaScaf` 的开发重点不是“改框架内部”，而是“用宿主项目按它的契约接入”。

AI 在没有源码的情况下，只要记住下面这几个关键点，就可以正常开展开发工作：

1. 它是一个后台框架库，不是完整应用。
2. 一切系统行为都围绕 `BsConfig` 注入。
3. 页面权限由“登录态 + 菜单/隐藏路由 + 角色”共同决定。
4. `BsConfig.Users` 是认证运行时缓存，必须和数据库同步。
5. 新页面开发时，路由与菜单配置必须同步。
6. `AddLogin`、日志委托、用户更新委托都必须真正落地。
7. 最佳实践是在宿主项目中扩展业务，而不是直接改框架底层。

如果按本手册实施，AI 即使拿不到 `BlaScaf` 源码，也能基于该框架快速搭建新的后台系统。
