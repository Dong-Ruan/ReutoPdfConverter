using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Spire.Pdf;
using Spire.Pdf.Graphics;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.International.Converters.PinYinConverter;
using Microsoft.Win32;

namespace ReutoPdfConverter
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // 自动注册右键菜单
            ReutoPdfConverter.RegisterContextMenu();

            if (args == null || args.Length == 0)
            {
                Console.WriteLine("未选择文件，程序已注册右键菜单。");
                return;
            }
            if (args == null || args.Length == 0)
            {

                return;
            }

            ReutoPdfConverter.ConvertFiles(args);
        }
    }

    public static class ReutoPdfConverter
    {
        //写入注册表
        public static void RegisterContextMenu()
        {
            try
            {
                string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string iconPath = Path.Combine(Path.GetDirectoryName(exePath), "pdfto2.ico");

                // 文件右键菜单
                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(@"*\shell\ReutoPDF"))
                {
                    key.SetValue("", "ReutoPDF");
                    key.SetValue("Icon", iconPath);
                    key.SetValue("MultiSelectModel", "Player");

                    using (RegistryKey cmd = key.CreateSubKey("command"))
                    {
                        cmd.SetValue("", $"\"{exePath}\" \"%1\"");
                    }
                }

                // 文件夹右键菜单
                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(@"Directory\shell\ReutoPDF"))
                {
                    key.SetValue("", "ReutoPDF");
                    key.SetValue("Icon", iconPath);
                    key.SetValue("MultiSelectModel", "Player");

                    using (RegistryKey cmd = key.CreateSubKey("command"))
                    {
                        cmd.SetValue("", $"\"{exePath}\" \"%1\"");
                    }
                }

                Console.WriteLine("右键菜单注册成功！");
            }
            catch (Exception ex)
            {
                Console.WriteLine("注册右键菜单失败：" + ex.Message);
            }
        }

        /// <summary>
        /// 自动转换：PDF->图片，图片->PDF
        /// </summary>
        public static void ConvertFiles(string[] files)
        {
            if (files == null || files.Length == 0)
            {
                Console.WriteLine("未选择文件！", "提示");
                return;
            }

            foreach (var file in files)
            {
                if (Directory.Exists(file))
                {
                    try
                    {
                        ConvertFolder(file);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"处理文件夹失败：{file}\n原因：{ex.Message}", "错误");
                    }
                }
                else
                {
                    string ext = Path.GetExtension(file).ToLower();
                    try
                    {
                        if (ext == ".pdf")
                            PDFtoIMG(file);
                        else if (ext == ".png" || ext == ".jpg" || ext == ".jpeg")
                            IMGtoPDF(file);
                        else
                            Console.WriteLine($"不支持的文件类型：{file}", "提示");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"处理文件失败：{file}\n原因：{ex.Message}", "错误");
                    }
                }
            }
        }

        /// <summary>
        /// 处理文件夹，将文件夹内所有图片合并为 PDF
        /// </summary>
        private static void ConvertFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException(folderPath);

            string[] images = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(f => new[] { ".png", ".jpg", ".jpeg" }.Contains(Path.GetExtension(f).ToLower()))
                .ToArray();

            if (images.Length == 0)
            {
                Console.WriteLine("文件夹中没有图片可转换！", "提示");
                return;
            }

            // 按排序规则
            Array.Sort(images, CompareFileName);

            string pdfPath = Path.Combine(folderPath, "合并转换_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".pdf");

            using (PdfDocument pdf = new PdfDocument())
            {
                foreach (var imgPath in images)
                {
                    using (System.Drawing.Image img = System.Drawing.Image.FromFile(imgPath))
                    {
                        PdfPageBase page = pdf.Pages.Add(new SizeF(img.Width, img.Height));
                        PdfImage pdfImg = PdfImage.FromImage(img);
                        page.Canvas.DrawImage(pdfImg, -40, -40, img.Width, img.Height);
                    }
                }
                pdf.SaveToFile(pdfPath);
            }

            Console.WriteLine($"文件夹中的图片已合并为 PDF:\n{pdfPath}", "完成");
        }

        /// <summary>
        /// PDF转图片
        /// </summary>
        private static void PDFtoIMG(string filePDF)
        {
            string pdfDir = Path.GetDirectoryName(filePDF);
            string pdfNameWithoutExt = Path.GetFileNameWithoutExtension(filePDF);
            string outputFolder = Path.Combine(pdfDir, pdfNameWithoutExt);

            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            using (PdfDocument pdf = new PdfDocument())
            {
                pdf.LoadFromFile(filePDF);

                for (int i = 0; i < pdf.Pages.Count; i++)
                {
                    using (System.Drawing.Image img = pdf.SaveAsImage(i, PdfImageType.Bitmap))
                    {
                        string imagePath = Path.Combine(outputFolder, $"Page_{i + 1}.png");
                        img.Save(imagePath, ImageFormat.Png);
                    }
                }
            }

            Console.WriteLine($"PDF 已成功转换为图片，存放在:\n{outputFolder}", "完成");
        }

        private static void IMGtoPDF(string imgPath)
        {
            if (!File.Exists(imgPath))
            {
                Console.WriteLine("图片文件不存在！", "错误");
                return;
            }

            string pdfPath = Path.Combine(Path.GetDirectoryName(imgPath), Path.GetFileNameWithoutExtension(imgPath) + ".pdf");

            using (System.Drawing.Image img = System.Drawing.Image.FromFile(imgPath))
            using (PdfDocument pdf = new PdfDocument())
            {
                PdfPageBase page = pdf.Pages.Add(new SizeF(img.Width, img.Height));

                PdfImage pdfImg = PdfImage.FromImage(img);

                // 绘制图片到页面，保持原始尺寸
                page.Canvas.DrawImage(pdfImg, -40, -40, img.Width, img.Height);

                pdf.SaveToFile(pdfPath);
            }

            Console.WriteLine($"图片已成功转换为 PDF:\n{pdfPath}", "完成");
        }

        /// <summary>
        /// 文件名排序：数字0-9 > A-Z > a-z > 汉字拼音
        /// </summary>
        private static int CompareFileName(string f1, string f2)
        {
            string name1 = Path.GetFileNameWithoutExtension(f1);
            string name2 = Path.GetFileNameWithoutExtension(f2);

            char c1 = name1.Length > 0 ? name1[0] : ' ';
            char c2 = name2.Length > 0 ? name2[0] : ' ';

            int rank1 = GetCharRank(c1);
            int rank2 = GetCharRank(c2);

            if (rank1 != rank2)
                return rank1 - rank2;

            // 汉字拼音比较
            if (rank1 == 4)
            {
                string py1 = GetFirstCharPinyin(c1);
                string py2 = GetFirstCharPinyin(c2);
                return string.Compare(py1, py2);
            }

            return string.Compare(name1, name2, StringComparison.OrdinalIgnoreCase);
        }

        private static int GetCharRank(char c)
        {
            if (char.IsDigit(c)) return 1;
            if (c >= 'A' && c <= 'Z') return 2;
            if (c >= 'a' && c <= 'z') return 3;
            if (c >= 0x4e00 && c <= 0x9fff) return 4;
            return 5;
        }

        private static string GetFirstCharPinyin(char c)
        {
            try
            {
                ChineseChar cc = new ChineseChar(c);
                return cc.Pinyins[0].Substring(0, 1).ToUpper();
            }
            catch
            {
                return c.ToString();
            }
        }
    }
}
