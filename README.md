# ReutoPdfConverter

ReutoPdfConverter 是一个 Windows 桌面命令行工具，可在 PDF 与常见图片格式之间转换，并可将文件夹中的图片合并为一个 PDF。

## 功能

- 将 PDF 的每一页导出为 PNG 图片。
- 将 PNG、JPG 或 JPEG 图片转换为 PDF。
- 将文件夹内的 PNG、JPG 和 JPEG 图片按文件名排序后合并为一个 PDF。
- 启动时尝试在文件和文件夹的右键菜单中注册“ReutoPDF”命令。
- 文件名排序支持中文拼音首字母。

## 运行环境

- Windows
- .NET Framework 4.7.2
- `Spire.Pdf.dll`（项目引用的 PDF 处理库）

## 使用方法

可以将一个或多个文件路径作为命令行参数传入：

```powershell
ReutoPdfConverter.exe "C:\Documents\report.pdf" "C:\Documents\photo.jpg"
```

也可以传入文件夹，将其中符合格式的图片合并成 PDF：

```powershell
ReutoPdfConverter.exe "C:\Documents\scans"
```

文件夹处理只读取该文件夹顶层的图片，不会递归处理子文件夹。

转换结果保存在源文件旁边：

- PDF 转图片：在同名文件夹中生成 `Page_1.png`、`Page_2.png` 等文件。
- 图片转 PDF：生成与图片同名的 PDF。
- 文件夹图片合并：生成带时间戳的 `合并转换_yyyyMMdd_HHmmss.pdf`。

每次启动程序时都会尝试写入 Windows 注册表，为文件和文件夹添加右键菜单项。若注册失败，程序会在控制台显示错误；也可以直接通过命令行传入路径使用转换功能。若目标位置已有同名输出文件，转换可能覆盖该文件。

## 从源码构建

1. 使用 Visual Studio 打开 `ReutoPdfConverter.sln`，并安装 .NET Framework 4.7.2 开发者工具包。
2. 还原 `packages.config` 中的 NuGet 依赖。
3. 将兼容的 `Spire.Pdf.dll` 放到项目引用所需的位置 `bin\Release\Spire.Pdf.dll`，或在 Visual Studio 中更新项目引用路径。
4. 构建解决方案。

`Spire.Pdf.dll` 不包含在此仓库的 Git 源码中。请自行获取该依赖，并遵守其适用的许可和使用条款。中文拼音排序依赖 `Microsoft.International.Converters.PinYinConverter` NuGet 包。

## 支持的输入

| 输入 | 输出 | 行为 |
| --- | --- | --- |
| `.pdf` | PNG 图片 | 每页输出一张图片 |
| `.png`、`.jpg`、`.jpeg` | PDF | 每张图片生成一个 PDF |
| 包含上述图片的文件夹 | PDF | 将顶层图片合并为一个 PDF |

其他文件格式不受支持。

## 许可证

当前仓库尚未提供许可证文件。如需复用、修改或分发本项目，请先确认仓库维护者声明的许可条件。
