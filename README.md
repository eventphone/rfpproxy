command to redirect without TPROXY:
iptables -t nat -I PREROUTING 1 -d 172.20.23.1 -p tcp --dport 16321 -j DNAT --to 172.20.23.1:16000
