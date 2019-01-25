#!/usr/bin/python36
from __future__ import print_function
import argparse
import socket

def parseCommandline():
    parser = argparse.ArgumentParser(description="RFP Led changer")
    parser.add_argument("-s, --socket", dest="socket", default="client.sock", help="socketproxy socket path", type=str)
    parser.add_argument("-m, --mac", dest="mac", help="RFP MAC address e.g. 0030420F8227", type=str, required=True)
    args = parser.parse_args()
    return args

def connectToProxy(args):
    s = socket.socket(socket.AF_UNIX, socket.SOCK_STREAM)
    s.connect(args.socket)

    greeting = s.recv(25)
    print(greeting)
    eos = b'{"type":"End","prio":0,"rfp":null,"filter":null}\n'
    s.sendall(eos)
    print (eos)
    end = s.recv(30)
    print(end)

    return s

args = parseCommandline()
sock = connectToProxy(args)

header = bytes.fromhex("00000013") + b"\0" + b"\0\0\0\0" + bytes.fromhex(args.mac)
messageprefix = bytes.fromhex("0102") + b"\0\4"

print("write led command to change colors")
print("first char identifies the led - second char identifies the color & pattern (0-7)")

while True:
    led = input("LED: ")
    if len(led) != 2:
        print("invalid pattern")
        continue
    msg = bytes.fromhex("0" + led[0]) + bytes.fromhex("0" + led[1]) + b"\0\0"
    sock.sendall(header)
    sock.sendall(messageprefix)
    sock.sendall(msg)