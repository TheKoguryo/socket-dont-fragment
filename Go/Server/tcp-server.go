package main

import (
	"fmt"
	"net"
	"os"
	"strconv"
	"strings"
	"syscall"
)

var PORT = "8080"
var BUFFER_SIZE = 5000
var DONT_FRAGMENT = false

func main() {
	if len(os.Args) > 1 {
		PORT = os.Args[1]
	}
	if len(os.Args) > 2 {
		boolValue, err := strconv.ParseBool(os.Args[2])
		if err != nil {
			fmt.Println(err)
			return
		}
		DONT_FRAGMENT = boolValue
	}

	// Listen for incoming connections on port 8080
	tcpAddr, _ := net.ResolveTCPAddr("tcp4", ":"+PORT)
	ln, err := net.ListenTCP("tcp", tcpAddr)
	if err != nil {
		fmt.Println(err)
		return
	}

	if DONT_FRAGMENT {
		err = setDontFragment(ln)
		if err != nil {
			fmt.Println(err)
			return
		}

		fmt.Println("DONT FRAGMENT is enabled")
	}

	// Accept incoming connections and handle them
	fmt.Println("Listening on " + PORT)
	for {
		conn, err := ln.Accept()
		if err != nil {
			fmt.Println(err)
			continue
		}

		// Handle the connection in a new goroutine
		go handleConnection(conn)
	}
}

func handleConnection(conn net.Conn) {
	// Close the connection when we're done
	defer conn.Close()

	// Read incoming data
	buffer := make([]byte, BUFFER_SIZE)
	data := ""

	for true {
		n, err := conn.Read(buffer)
		if err != nil {
			fmt.Println(err)
			return
		}

		data += string(buffer[:n])

		if strings.Contains(data, "<EOF>") {
			break
		}
	}

	// Print the incoming data
	fmt.Printf("[%s] Received: %s\n", conn.RemoteAddr(), data)

	message := strconv.Itoa(len(data)) + " bytes received"
	_, err := conn.Write([]byte(message))
	if err != nil {
		fmt.Println(err)
		return
	}
	fmt.Printf("[%s] Sent: %s\n", conn.RemoteAddr(), message)
}

// Reference: https://github.com/xaionaro-go/udpnofrag/blob/master/udp_linux.go
func setDontFragment(ln *net.TCPListener) (err error) {
	var syscallConn syscall.RawConn
	syscallConn, err = ln.SyscallConn()
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
