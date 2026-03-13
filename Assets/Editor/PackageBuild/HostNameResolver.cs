using System;
using System.Net;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

public class HostNameResolver
{
    public static IPHostEntry GetHostEntrySafe(string hostName = null)
    {
        try
        {
            // 如果没有提供主机名，使用安全方式获取
            hostName ??= GetSafeHostName();
            
            // 检查是否包含非ASCII字符
            if (ContainsNonAsciiCharacters(hostName))
            {
                // 使用IdnMapping将国际化域名转换为ASCII格式
                IdnMapping idn = new IdnMapping();
                string asciiHostName = idn.GetAscii(hostName);
                return Dns.GetHostEntry(asciiHostName);
            }
            else
            {
                return Dns.GetHostEntry(hostName);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DNS解析失败: {ex.Message}");
            return GetLocalHostEntryFallback();
        }
    }
    
    private static bool ContainsNonAsciiCharacters(string input)
    {
        foreach (char c in input)
        {
            if (c > 127)
                return true;
        }
        return false;
    }
    
    private static string GetSafeHostName()
    {
        // 使用Environment.MachineName替代Dns.GetHostName()
        return Environment.MachineName;
    }
    
    private static IPHostEntry GetLocalHostEntryFallback()
    {
        // 备用方案：手动构建IPHostEntry
        IPHostEntry hostEntry = new IPHostEntry
        {
            HostName = Environment.MachineName,
            Aliases = Array.Empty<string>()
        };
        
        // 获取所有本地IP地址
        var addresses = new List<IPAddress>();
        
        try
        {
            // 方法1：通过网络接口获取IP
            var networkInterfaces = System.Net.NetworkInformation.NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == 
                    System.Net.NetworkInformation.OperationalStatus.Up);
            
            foreach (var ni in networkInterfaces)
            {
                var ipProps = ni.GetIPProperties();
                foreach (var addr in ipProps.UnicastAddresses)
                {
                    if (addr.Address.AddressFamily == AddressFamily.InterNetwork) // IPv4
                    {
                        addresses.Add(addr.Address);
                    }
                }
            }
        }
        catch
        {
            // 方法2：如果网络接口获取失败，使用回退地址
            addresses.Add(IPAddress.Loopback);  // 127.0.0.1
            addresses.Add(IPAddress.IPv6Loopback);  // ::1
        }
        
        hostEntry.AddressList = addresses.ToArray();
        return hostEntry;
    }
}