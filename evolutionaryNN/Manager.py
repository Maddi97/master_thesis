import numpy as np

import Communicator
from socket import socket, AF_INET, SOCK_DGRAM
import threading


class CommunicationManager:
    def __init__(self, port, address, botcount):
        self.port = port
        self.address = address
        self.results = None
        self.running = False
        self.botcount = botcount
        self.changedbotcount = True
        self.newresultpls = False
        self.shuffle = False
        self.eog = False
        self.start = False
        self.resultsarethere = False
        self.sender = threading.Thread(target=self.sender)
        self.receiver = threading.Thread(target=self.reciver)
        self.startcommunication()
        self.botlist = []

    def setbotcount(self, count):
        self.botcount = count
        self.changedbotcount = True

    def checkresults(self):
        if self.resultsarethere:
            self.resultsarethere = False
            return True
        return False

    def setstart(self):
        self.start = True

    def setstop(self):
        self.eog = True

    def askforresult(self):
        self.newresultpls = True

    def setshuffle(self):
        self.shuffle = True

    def getresults(self):
        res = self.results
        self.results = None
        return res

    def startcommunication(self):
        self.sender.start()
        self.receiver.start()

    def reciver(self):
        sock = socket(AF_INET, SOCK_DGRAM)
        sock.bind((self.address, self.port + 3))
        sock.settimeout(5)
        results = ""
        resultsevent = False
        while True:
            if self.running:
                break
            try:
                msg, addr = sock.recvfrom(8654)  # This is the amount of bytes to read at maximum
                #if msg == bytes("RESULTSTART", 'utf-8'):
                #    resultsevent = True
                #    results = ""
                #    continue
                #if msg == bytes("RESULTEND", 'utf-8'):

                results = msg.decode('utf-8')
                results = results.split(";")
                self.results = np.array(results).astype(float)
                self.resultsarethere = True

            except:
                pass

    def sender(self):
        sock = socket(AF_INET, SOCK_DGRAM)
        sock.bind((self.address, self.port + 1))
        while True:
            if self.running:
                break
            if self.shuffle:
                sock.sendto(bytes("SHUFFLE", "utf-8"), (self.address, self.port + 2))
                self.shuffle = False
            if self.start:
                print("sendstart")
                sock.sendto(bytes("START", "utf-8"), (self.address, self.port + 2))
                self.start = False
            if self.changedbotcount:
                sock.sendto(bytes("BOTCOUNT;" + str(self.botcount), "utf-8"), (self.address, self.port + 2))
                self.changedbotcount = False
            if self.eog:
                sock.sendto(bytes("EOG", "utf-8"), (self.address, self.port + 2))
                self.eog = False
            if self.newresultpls:
                sock.sendto(bytes("GETRESULT", "utf-8"), (self.address, self.port + 2))
                print("askingagain")
                self.newresultpls = False
