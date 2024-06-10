package main

import (
	"fmt"
	"net"
	"os"
	"strconv"
	"syscall"
)

var SERVER = "150.xxx.xxx.xx"
var PORT = "8080"
var BUFFER_SIZE = 1024
var MSG_SIZE = 6000
var DONT_FRAGMENT = true

func main() {
	if len(os.Args) > 1 {
		SERVER = os.Args[1]
	}
	if len(os.Args) > 2 {
		PORT = os.Args[2]
	}
	if len(os.Args) > 3 {
		boolValue, err := strconv.ParseBool(os.Args[3])
		if err != nil {
			fmt.Println(err)
			return
		}
		DONT_FRAGMENT = boolValue
	}

	// Connect to the server
	tcpAddr, _ := net.ResolveTCPAddr("tcp4", SERVER+":"+PORT)
	conn, err := net.DialTCP("tcp", nil, tcpAddr)
	if err != nil {
		fmt.Println(err)
		return
	}

	fmt.Printf("Socket connected to %s\n", conn.RemoteAddr())

	if DONT_FRAGMENT {
		err = setDontFragment(conn)
		if err != nil {
			fmt.Println(err)
			return
		}

		fmt.Println("DontFragment(DF) is enabled")
	}

	message := strconv.Itoa(MSG_SIZE)
	paddedMessage := padRight(message, "_", MSG_SIZE-len("<EOF>")) + "<EOF>"

	// Send some data to the server
	//n, err = conn.Write([]byte("Hello, server!"))
	n := 0
	n, err = conn.Write([]byte(paddedMessage))
	if err != nil {
		fmt.Println(err)
		return
	}

	fmt.Printf("Sent: %s bytes\n", strconv.Itoa(n))
	fmt.Println(paddedMessage)

	buffer := make([]byte, BUFFER_SIZE)
	n, err = conn.Read(buffer)
	if err != nil {
		fmt.Println(err)
		return
	}
	fmt.Printf("Message from Server: %s\n", string(buffer[:n]))

	// Close the connection
	conn.Close()
}

// https://github.com/xaionaro-go/udpnofrag/blob/master/udp_linux.go
func setDontFragment(conn *net.TCPConn) (err error) {
	var syscallConn syscall.RawConn
	syscallConn, err = conn.SyscallConn()
	if err != nil {
		return
	}
	err2 := syscallConn.Control(func(fd uintptr) {
		err = syscall.SetsockoptByte(int(fd), syscall.IPPROTO_IP, syscall.IP_MTU_DISCOVER, syscall.IP_PMTUDISC_DO)
	})
	if err != nil {
		return
	}
	err = err2
	return
}

func padRight(str, pad string, lenght int) string {
	for {
		str += pad
		if len(str) > lenght {
			return str[0:lenght]
		}
	}
}
