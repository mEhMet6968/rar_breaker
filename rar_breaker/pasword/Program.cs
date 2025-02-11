using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace RarPasswordBruteForce
{
	class Program
	{
		static readonly char[] charset = "0123456789".ToCharArray();
		//é\"!'^+%&/()=?_>£#$½{[]}|\\-*@€,;.:abcdefghijklmnopqrstuvwxyzUVWXYZDEFGHIJKLMNOPQCRS89A6042
		//you can weite any char
		static void Main(string[] args)
		{
			Console.Write("Şifreli RAR dosyasının tam yolunu girin (örnek: C:\\Dosyalar\\dosya.rar): ");
			string filename = Console.ReadLine();

			if (!File.Exists(filename))
			{
				Console.WriteLine("Dosya bulunamadı.");
				return;
			}

			Console.Write("Çıkartılacak dosya yolunu girin (örnek: C:\\Dosyalar\\Çıkartılanlar): ");
			string outputFolder = Console.ReadLine();

			if (!Directory.Exists(outputFolder))
			{
				Console.WriteLine("Girilen klasör bulunamadı, yeni klasör oluşturuluyor...");
				Directory.CreateDirectory(outputFolder);
			}

			// Geçici klasör oluştur
			string tmpFolder = "TempExtract";
			Directory.CreateDirectory(tmpFolder);

			// Şifre deneme kombinasyonlarını oluştur ve test et
			bool passwordFound = false;
			for (int length = 1; length <= 5 && !passwordFound; length++)
			{
				passwordFound = TryBruteForce(filename, tmpFolder, outputFolder, length);
				if (passwordFound)
				{
					Console.WriteLine("Şifre bulundu!");
					//MoveFilesToOutputFolder(tmpFolder, outputFolder);
				}
			}

			// Temizleme
			Directory.Delete(tmpFolder, true);
			Console.WriteLine("İşlem tamamlandı.");
		}

		static bool TryBruteForce(string filename, string tmpFolder, string outputFolder, int length)
		{
			StringBuilder password = new StringBuilder(new string(charset[0], length));
			return BruteForceRecursive(filename, tmpFolder, outputFolder, password, 0, length);
		}

		static bool BruteForceRecursive(string filename, string tmpFolder, string outputFolder, StringBuilder password, int position, int length)
		{
			if (position == length)
			{
				return TryPassword(filename, tmpFolder, password.ToString());
			}

			for (int i = 0; i < charset.Length; i++)
			{
				password[position] = charset[i];
				if (BruteForceRecursive(filename, tmpFolder, outputFolder, password, position + 1, length))
					return true;
			}
			return false;
		}

		static bool TryPassword(string filename, string tmpFolder, string password)
		{
			Console.WriteLine("Deneniyor: " + password);

			// Geçici klasörü temizle
			if (Directory.Exists(tmpFolder))
				Directory.Delete(tmpFolder, true);
			Directory.CreateDirectory(tmpFolder);

			// WinRAR komutunu ayarla
			var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = @"C:\Program Files\WinRAR\rar.exe",
					Arguments = $"x -inul -p{password} \"{filename}\" \"{tmpFolder}\"",
					RedirectStandardOutput = true,
					UseShellExecute = false,
					CreateNoWindow = true
				}
			};

			process.Start();
			process.WaitForExit();

			// Eğer çıkış kodu 0 ise, şifre doğrudur
			if (process.ExitCode == 0)
			{
				Console.WriteLine($"Şifre Bulundu: {password}");
				return true;
			}
			else
			{
				// Hatalı dosya mesajı
				if (process.ExitCode == 2)
				{
					Console.WriteLine($"Hata: Dosya açılamıyor. Şifre: {password} yanlış.");
				}
				else
				{
					Console.WriteLine($"Hata: WinRAR işlemi başarısız oldu, çıkış kodu: {process.ExitCode}");
				}
			}

			return false;
		}

		static void MoveFilesToOutputFolder(string tmpFolder, string outputFolder)
		{
			// Geçici klasörde dosya varsa, dosyaları hedef klasöre taşı
			if (Directory.GetFiles(tmpFolder).Length > 0)
			{
				foreach (var file in Directory.GetFiles(tmpFolder))
				{
					string destFile = Path.Combine(outputFolder, Path.GetFileName(file));
					File.Move(file, destFile);
				}
				Console.WriteLine("Dosyalar başarıyla taşındı.");
			}
			else
			{
				Console.WriteLine("Taşınacak dosya yok.");
			}
		}
	}
}