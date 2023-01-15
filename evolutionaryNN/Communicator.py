import sys
import time
from socket import socket, AF_INET, SOCK_DGRAM, SO_SNDBUF, SOL_SOCKET
import numpy as np
import cv2
import threading
import NeuralNetwork
import multiprocessing


class Communicator:
    def __init__(self, port: int, address: str, neuralnet: NeuralNetwork):
        self.port = port
        self.address = address
        self.neuralnet = neuralnet
        self.running = True
        self.dataready = False
        self.datatobesend: str = ""
        self.sender = threading.Thread(target=self.sender)
        self.receiver =   threading.Thread(target=self.receiver)
        self.sender.start()
        self.receiver.start()

    def setstoprunning(self):
        self.running = False

    def stopthread(self):
        self.sender.join()
        self.receiver.join()

    def receiver(self):
        sock = socket(AF_INET, SOCK_DGRAM)
        sock.setsockopt(SOL_SOCKET, SO_SNDBUF, 17308)
        sock.bind((self.address, self.port + 3))
        sock.settimeout(5)
        pic = []
        begon = False
        while True:
            if not self.running:
                break
            try:
                msg, addr = sock.recvfrom(8654)  # This is the amount of bytes to read at maximum
                if msg == bytes("ENDOFGAME", 'utf-8'):
                    break
                if msg == bytes("LosGehtsKleinerHase", 'utf-8'):
                    pic = []
                    begon = True
                    continue
                if msg == bytes("ZuEndekleinerHase", 'utf-8'):
                    begon = False
                    self.imagetoposition(bytearray(pic))
                    continue
                if begon:
                    pic += msg
            except Exception as e:
                print(e)

    def sender(self):
        sock = socket(AF_INET, SOCK_DGRAM)
        sock.bind((self.address, self.port + 1))
        while True:
            if not self.running:
                sock.sendto(bytes("ENDREG", "utf-8"), (self.address, self.port + 2))
                break
            if self.dataready:


                sock.sendto(bytes(self.datatobesend, "utf-8"), (self.address, self.port + 2))
                print(f"senddata {self.datatobesend} sender_port: {self.port}")

                self.dataready = False

    def imagetoposition(self, image: bytes):
        img = cv2.imdecode(np.asarray(image, dtype=np.uint8), cv2.IMREAD_UNCHANGED)
        cv2.imwrite('./images/test.png', img)
        height, width, _ = img.shape
        hsv = cv2.cvtColor(img, cv2.COLOR_BGR2HSV)
        lower_band = np.array([3, 106, 0])
        upper_bande = np.array([60, 202, 165])
        bande = cv2.inRange(hsv, lower_band, upper_bande)
        ret, bande_img = cv2.threshold(bande, 70, 255, cv2.THRESH_BINARY)
        cv2.imwrite('./images/boundaries.png', bande_img)

        #lower_red = np.array([0, 0, 125])
        #upper_red = np.array([179, 242, 131])
        lower_red = np.array([4, 0, 19])
        upper_red = np.array([136, 234, 205])
        red = cv2.inRange(hsv, lower_red, upper_red)

        ret, red_img = cv2.threshold(red, 70, 255, cv2.THRESH_BINARY)
        cv2.imwrite('./images/redObstacles.png', red_img)

        upper_blue = np.array([114, 231, 138], np.uint8)
        lower_blue = np.array([110, 0, 99], np.uint8)
        blue = cv2.inRange(hsv, lower_blue, upper_blue)
        ret, blue_img = cv2.threshold(blue, 70, 255, cv2.THRESH_BINARY)
        cv2.imwrite('./images/blueObstacles.png', blue_img)

        contours_blue, _ = cv2.findContours(blue_img, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)
        contours_red, _ = cv2.findContours(cv2.bitwise_not(red_img), cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)
        contours_bande, _ = cv2.findContours(bande_img, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)
        redreq = []
        bluereg = []
        bandreg = []
        for cnt in contours_red:
            # Calculate area and remove small elements
            area = cv2.contourArea(cnt)
            if area > 10:
                (x, y, w, h) = cv2.boundingRect(cnt)
                bound = np.asarray([x, y, w, h])
                redreq.append(bound)

        for cnt in contours_blue:
            area = cv2.contourArea(cnt)
            if area > 10:
                (x, y, w, h) = cv2.boundingRect(cnt)
                bound = np.asarray([x, y, w, h])
                bluereg.append(bound)

        for cnt in contours_bande:
            area = cv2.contourArea(cnt)
            if area > 10:
                (x, y, w, h) = cv2.boundingRect(cnt)
                bound = np.asarray([x, y, w, h])
                bandreg.append(bound)
        allContours = [redreq, bluereg, bandreg]
        t = self.neuralnet.neuralnet(allContours)
        # t = [.9 , .1]

        print(f"Port: {self.port} data: [{t[0]}, {t[1]}]")
        self.datatobesend = ';'.join(str(x) for x in t)
        self.dataready = True

