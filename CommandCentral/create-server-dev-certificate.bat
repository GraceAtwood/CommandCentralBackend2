makecert.exe -r -pe -n "CN=localhost" -sky exchange -sv server.pvk server.cer
pvk2pfx -pvk server.pvk -spc server.cer -pfx server.pfx -pi password