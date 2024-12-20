
using System.IO.Compression;
using System.Diagnostics;
using OfficeOpenXml;

namespace GZipCompressionApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtFolderPath.Text = folderDialog.SelectedPath;
                }
            }
        }
        private void btnCompress_Click(object sender, EventArgs e)
        {
            string folderPath = txtFolderPath.Text;

            if (Directory.Exists(folderPath))
            {
                lblStatus.Text = "Duke kompresuar...";
                lblStatus.ForeColor = Color.Black;
                Task.Run(() =>
                {
                    string zipFilePath = CompressFolderToZip(folderPath);
                    Invoke(new Action(() =>
                    {
                        lblStatus.Text = "Kompresimi përfundoi!";
                        lblStatus.ForeColor = Color.Green;
                        txtFolderPath.Clear();
                    }));
                });
            }
            else
            {
                MessageBox.Show("Ju lutem zgjidhni një folder të vlefshëm.", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private static string CompressFolderToZip(string folderPath)
        {
            try
            {
                string zipFilePath = folderPath + ".zip";
                if (File.Exists(zipFilePath))
                {
                    File.Delete(zipFilePath);
                }

                var files = Directory.GetFiles(folderPath);
                string tempDir = Path.Combine(folderPath, "tempZipFiles");
                Directory.CreateDirectory(tempDir);

                var results = new List<(string Method, long Time)>();

                Stopwatch stopwatch = Stopwatch.StartNew();
                CompressWithOneThread(files, tempDir);
                stopwatch.Stop();
                results.Add(("One Thread", stopwatch.ElapsedMilliseconds));

                stopwatch.Restart();
                Parallel.ForEach(files, (file) =>
                {
                    CompressFileToTempZip(file, tempDir);
                });
                stopwatch.Stop();
                results.Add(("Multi Thread", stopwatch.ElapsedMilliseconds));

                SaveCompressionResultsToExcel(results);

                CombineZipsIntoSingleZip(tempDir, zipFilePath);

                Directory.Delete(tempDir, true);

                return zipFilePath;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë kompresimit: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return string.Empty;
            }
        }

        private static void CompressWithOneThread(string[] files, string tempDir)
        {
            foreach (var file in files)
            {
                CompressFileToTempZip(file, tempDir);
            }
        }

        private static void CompressFileToTempZip(string filePath, string tempDir)
        {
            try
            {
                string tempZipPath = Path.Combine(tempDir, Path.GetFileName(filePath) + ".zip");

                using (FileStream zipToCreate = new FileStream(tempZipPath, FileMode.Create))
                {
                    using (ZipArchive archive = new ZipArchive(zipToCreate, ZipArchiveMode.Update))
                    {
                        string fileName = Path.GetFileName(filePath);
                        ZipArchiveEntry entry = archive.CreateEntry(fileName);
                        using (Stream entryStream = entry.Open())
                        {
                            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                            {
                                fileStream.CopyTo(entryStream);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë kompresimit të file-it: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void CombineZipsIntoSingleZip(string tempDir, string zipFilePath)
        {
            try
            {
                using (FileStream zipToCreate = new FileStream(zipFilePath, FileMode.Create))
                {
                    using (ZipArchive archive = new ZipArchive(zipToCreate, ZipArchiveMode.Create))
                    {
                        var tempFiles = Directory.GetFiles(tempDir);
                        foreach (var tempFile in tempFiles)
                        {
                            string fileName = Path.GetFileName(tempFile);
                            ZipArchiveEntry entry = archive.CreateEntry(fileName);

                            using (Stream entryStream = entry.Open())
                            {
                                using (FileStream fileStream = new FileStream(tempFile, FileMode.Open, FileAccess.Read))
                                {
                                    fileStream.CopyTo(entryStream);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë bashkimit të zip file-ve: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void SaveCompressionResultsToExcel(List<(string Method, long Time)> results)
        {
            try
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "CompressionResults.xlsx");

                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets.Add("Simulime");

                    worksheet.Cells[1, 1].Value = "Simulimi";
                    worksheet.Cells[1, 2].Value = "Koha (ms)";

                    int row = 2;
                    foreach (var result in results)
                    {
                        worksheet.Cells[row, 1].Value = result.Method;
                        worksheet.Cells[row, 2].Value = result.Time;
                        row++;
                    }

                    package.Save();
                }

                MessageBox.Show("Rezultatet u ruajtën në Desktop!", "Përfunduar", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë ruajtjes në Excel: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDecompress_Click(object sender, EventArgs e)
        {
            using (var fileDialog = new OpenFileDialog())
            {
                fileDialog.Filter = "ZIP Files|*.zip";

                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    string zipFilePath = fileDialog.FileName;
                    string destinationPath = Path.Combine(Path.GetDirectoryName(zipFilePath), Path.GetFileNameWithoutExtension(zipFilePath));

                    lblStatus.Text = "Duke dekompresuar...";
                    lblStatus.ForeColor = Color.Black;
                    Task.Run(() =>
                    {
                        Stopwatch stopwatch = Stopwatch.StartNew();
                        DecompressWithOneThread(zipFilePath, destinationPath);
                        stopwatch.Stop();
                        long oneThreadTime = stopwatch.ElapsedMilliseconds;

                        stopwatch.Restart();
                        DecompressWithMultiThread(zipFilePath, destinationPath);
                        stopwatch.Stop();
                        long multiThreadTime = stopwatch.ElapsedMilliseconds;

                        SaveDecompressionResultsToExcel(zipFilePath, destinationPath, oneThreadTime, multiThreadTime);

                        Invoke(new Action(() =>
                        {
                            lblStatus.Text = "Dekomprimimi përfundoi!";
                            lblStatus.ForeColor = Color.Green;
                        }));
                    });
                }
            }
        }

        private void DecompressWithOneThread(string zipFilePath, string destinationPath)
        {
            try
            {
                System.IO.Compression.ZipFile.ExtractToDirectory(zipFilePath, destinationPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë dekompresimit me një thread: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DecompressWithMultiThread(string zipFilePath, string destinationPath)
        {
            try
            {
                System.IO.Compression.ZipFile.ExtractToDirectory(zipFilePath, destinationPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë dekompresimit me shumë threads: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void SaveDecompressionResultsToExcel(string zipFilePath, string destinationPath, long oneThreadTime, long multiThreadTime)
        {
            try
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "DecompressionResults.xlsx");

                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets.Add("Decompression Results");

                    worksheet.Cells[1, 1].Value = "ZIP File";
                    worksheet.Cells[1, 2].Value = "Destination Path";
                    worksheet.Cells[1, 3].Value = "One Thread Time (ms)";
                    worksheet.Cells[1, 4].Value = "Multi Thread Time (ms)";

                    worksheet.Cells[2, 1].Value = zipFilePath;
                    worksheet.Cells[2, 2].Value = destinationPath;
                    worksheet.Cells[2, 3].Value = oneThreadTime;
                    worksheet.Cells[2, 4].Value = multiThreadTime;

                    package.Save();
                }

                MessageBox.Show("Dekomprimimi dhe rezultatet janë ruajtur në Excel!", "Përfunduar", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gabim gjatë ruajtjes së rezultatit të dekompresimit në Excel: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
  }