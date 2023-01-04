import json
import random
import math
import sys
import threading
import time

import numpy as np
from typing import List
import Communicator
import Manager


class NeuralNet:
    # initiallise a neural net. Layer beeing an array, where every position indicates how many neurons this layer has
    # the first and last position of the list are the input and output layers of the neural net
    def __init__(self, layer: List[List], encodednet=np.empty(0), upperbound=1, lowerbound=-1, performance=None,
                 elter=None, age=0):
        if elter is None:
            elter = []
        self.layer = layer
        self.age = age
        self.Neuralnetlengh = self.caculatelnght()
        self.offset = self.calcoffset()
        self.performance = performance
        self.neuron = []
        self.elter = elter
        self.encodednet = encodednet
        self.endsofnns = self.setends()
        if len(encodednet) > 0:
            self.calculatelowerandupperbound()
        else:
            self.upperbound = upperbound
            self.lowerbound = lowerbound

    def caculatelnght(self):
        count = 0
        for x in self.layer:
            neuroncount = sum(x)
            count += neuroncount + pow(neuroncount, 2) - x[0]
        return count

    def calculatelowerandupperbound(self):
        self.lowerbound = np.min(self.encodednet)
        self.upperbound = np.max(self.encodednet)

    def initvalues(self):
        lenngh = 0
        for nn in self.layer:
            lenngh += sum(nn[1:])
            for i in range(len(nn)):
                if i + 1 == len(nn):
                    continue
                lenngh += nn[i] * nn[i + 1]
        self.encodednet = np.random.uniform(low=self.lowerbound, high=self.upperbound, size=lenngh)
        self.calculatelowerandupperbound()
        print(len(self.encodednet))

    def setends(self):
        begins = []
        for nn in self.layer:
            lenngh = 0
            lenngh += sum(nn[1:])
            for i in range(len(nn)):
                if i + 1 == len(nn):
                    continue
                lenngh += nn[i] * nn[i + 1]
            if len(begins) > 0:
                begins.append(begins[-1] + lenngh)
            else:
                begins.append(lenngh)
        return begins

    def calcoffset(self):
        count = sum(self.layer[0])
        return count + pow(count, 2) - self.layer[0][0]

    def getperformance(self):
        return self.performance

    def getupperbound(self):
        return self.upperbound

    def getage(self):
        return self.age

    def getelter(self):
        return self.elter

    def getlowerbound(self):
        return self.lowerbound

    def getencoded(self):
        return self.encodednet

    def setencoded(self, encoded):
        self.encodednet = encoded

    def setperfromance(self, performance):
        self.performance = performance

    # reduces the deminesion of the neual network
    # to allow tweeking of importance from differnt demensions, these neurons have the possiblility to tweek the bias
    # for each demension, while the weight is staying the same
    def reducedeminesion(self, input, bias):
        out = 0
        for x, y in zip(input, bias):
            out += input * bias
        return out

    def createoppositeindividual(self, pos, age):
        oppositeencodednet = np.empty(0)
        for x in self.encodednet:
            value = random.uniform((self.upperbound + self.lowerbound) / 2, self.lowerbound + self.upperbound - x)
            oppositeencodednet = np.append(oppositeencodednet, value)
        return NeuralNet(self.layer, oppositeencodednet, elter=[pos], age=age)

    def feedforward(self, input, layer: List, index, offset=0):
        neuron = []
        for x in input:
            neuron.append(x)
        if offset == 0:
            weightcounter = 0
        else:
            weightcounter = self.endsofnns[offset - 1] + 1
        biascounter = self.endsofnns[offset] - sum(layer[1:])
        neuroncounter = 0
        for x in range(len(layer)):
            value = 0
            if x + 1 < len(layer):
                for i in range(layer[x + 1]):
                    for t in range(layer[x]):
                        value += neuron[neuroncounter] * self.encodednet[weightcounter]
                        weightcounter += 1
                    neuroncounter += 1
                    neuron.append(math.tanh(value + self.encodednet[biascounter]))
                    biascounter += 1
        # try:
        return neuron[-layer[-1]:]
        # except Exception as e:
        #    print(e)
        #   print(f"Index is: {index} and lenght is: {len(self.neuron)}")



    def neuralnet(self, inputdata: List[List]):
        """
        inputdata: list from image obstacle recognition pipeline (obstacle contour list)
        """
        index = 0
        nnposition = 0
        arraylen = sum(len(c) for c in inputdata)
        self.neuron = []
        # print(f"list lengh is {len(self.neuron)}")
        for p in inputdata:
            for t in p:
                self.neuron.append(self.feedforward(t, self.layer[nnposition], index, nnposition))
                # x = threading.Thread(target=self.feedforward, args=(t, self.layer[nnposition], index, nnposition))
                index += 1
                # x.start()
                # threds.append(x)
            nnposition += 1
        # for t in threds:
        #    t.join()
        newinput = np.empty(0)
        inputcounter = 0
        # del t, x
        for x in inputdata:
            temparray = np.zeros(self.layer[0][-1])
            to = len(x)
            for neuroncounter in range(inputcounter, to):
                try:
                    temparray = np.add(temparray, self.neuron[neuroncounter])
                except Exception as e:
                    print(e)
                    print(f"this neuron{self.neuron} this tempattay {temparray} and the  {self.layer}")
            inputcounter += len(x) - 1
            newinput = np.concatenate((newinput, temparray))

        # TODO BUG????
        prediction = self.feedforward(newinput, self.layer[nnposition], 0, nnposition)
        pred2 = self.neuron[0]
        return prediction
        # return


class CENDEDOBL:
    def __init__(self, populationsize: List[NeuralNet], jumpingrate, runtime, layer, startport, chunksize,
                 save: str = "./",
                 address="127.0.0.1"):
        self.manager = None
        self.populationsize = populationsize
        self.jumpingrate = jumpingrate
        self.lowerbound = 1
        self.runtime = runtime
        self.upperbound = -1
        self.startport = startport
        self.layer = layer
        self.save = save
        self.chunksize = chunksize
        self.iteration = 0
        self.address = address

    def starmanger(self):
        self.manager = Manager.CommunicationManager(self.startport, self.address, self.chunksize)
        #self.manager.setshuffle()
        self.startport += 4

    def getpop(self):
        return self.populationsize

    def writedata(self):
        data = {"layer": self.layer,
                'jumpingrate': self.jumpingrate,
                "lowerbound": self.lowerbound,
                "upperbound": self.upperbound,
                "interation": self.iteration
                }
        nets = []
        performance = []
        boundries = []
        elter = []
        age = []
        for x in self.populationsize:
            nets.append(x.getencoded().tolist())
            performance.append(x.getperformance())
            boundries.append([x.getupperbound(), x.getlowerbound()])
            elter.append(x.getelter())
            age.append(x.getage())
        data["individuals"] = nets
        data["performance"] = performance
        data["boundries"] = boundries
        data["elter"] = elter
        data["age"] = age
        with open(self.save + str(self.iteration) + ".json", "w") as outfile:
            json.dump(data, outfile)
        self.iteration += 1
        # if self.iteration == 50:
        #    self.runtime += 10

    def readindata(self, filename: str):
        data = json.load(open(filename))
        self.layer = data['layer']
        self.jumpingrate = data['jumpingrate']
        self.lowerbound = data['lowerbound']
        self.upperbound = data['upperbound']
        self.iteration = data['interation']
        performance = data["performance"]
        individual = data["individuals"]
        boundries = data["boundries"]
        try:
            elter = data["elter"]

        except:
            elter = [None] * len(individual)
        try:
            age = data["age"]
        except:
            age = [0] * len(individual)
        self.populationsize = []
        for x in range(len(individual)):
            self.populationsize.append(
                NeuralNet(self.layer, np.array(individual[x]), upperbound=boundries[x][0], lowerbound=boundries[x][1],
                          performance=performance[x], elter=elter[x], age=age[x]))
        self.iteration += 1

    # if self.iteration > 50:
    #    self.runtime += 10
    # if self.iteration > 84:
    #    self.runtime += 10
    # if self.iteration > 100:
    #    self.runtime += 10

    def lowerandupperbound(self):
        for x in self.populationsize:
            self.upperbound = x.getupperbound() if x.getupperbound() > self.upperbound else self.upperbound
            self.lowerbound = x.getlowerbound() if x.getlowerbound() < self.lowerbound else self.lowerbound
        print(f"self upper {self.upperbound} self lower {self.lowerbound}")

    def obl(self):
        self.lowerandupperbound()
        o_pop: List[NeuralNet] = []
        i = 0
        for x in self.populationsize:
            o_pop.append(x.createoppositeindividual(i, self.iteration))
            i += 1
        self.findbestindividuals(o_pop)

    def sortfunc(self, neuralnet: NeuralNet):
        return neuralnet.getperformance()

    def benchmark(self, rounds: int):

        finresult = []
        for kk in range(rounds):
            counter = 0
            print(len(self.populationsize))
            roundresult = []
            while counter < len(self.populationsize):
                startport = self.startport
                currentactiveingame = []
                if len(self.populationsize) - counter > self.chunksize:
                    self.manager.setbotcount(self.chunksize)
                    self.manager.setstart()
                    for x in range(self.chunksize):
                        currentactiveingame.append(
                            Communicator.Communicator(startport, self.address, self.populationsize[counter]))
                        counter += 1
                        startport += 4
                else:
                    self.manager.setbotcount(len(self.populationsize) - counter)
                    self.manager.setstart()
                    for x in range(len(self.populationsize) - counter):
                        currentactiveingame.append(
                            Communicator.Communicator(startport, self.address, self.populationsize[counter]))
                        counter += 1
                        startport += 4
                time.sleep(self.runtime)
                self.manager.setstop()
                print("setstop")
                for active in currentactiveingame:
                    active.setstoprunning()
                for active in currentactiveingame:
                    active.stopthread()
                results = self.manager.getresults()
                while results is None:
                    #     self.manager.askforresult()
                    print("Waitingforresults")
                    #    time.sleep(1)
                    results = self.manager.getresults()
                roundresult.extend(results)
                print(results)
            finresult.append(roundresult)
        return finresult

    def findbestindividuals(self, coparer: List[NeuralNet], onebyone=False, testself=False):
        counter = 0
        while counter < len(coparer):
            startport = self.startport
            currentactiveingame = []
            if len(coparer) - counter > self.chunksize:
                self.manager.setbotcount(self.chunksize)
                self.manager.setstart()
                for x in range(self.chunksize):
                    currentactiveingame.append(Communicator.Communicator(startport, self.address, coparer[counter]))
                    counter += 1
                    startport += 4
            else:
                self.manager.setbotcount(len(coparer) - counter)
                self.manager.setstart()
                for x in range(len(coparer) - counter):
                    currentactiveingame.append(Communicator.Communicator(startport, self.address, coparer[counter]))
                    counter += 1
                    startport += 4
            time.sleep(self.runtime)
            self.manager.setstop()
            print("setstop")
            for active in currentactiveingame:
                active.setstoprunning()
            for active in currentactiveingame:
                active.stopthread()
            results = self.manager.getresults()
            while results is None:
                #     self.manager.askforresult()
                print("Waitingforresults")
                #    time.sleep(1)
                results = self.manager.getresults()
            print(results)
            x = 0
            for res in range(len(results), 0, -1):
                coparer[counter - res].setperfromance(results[x])
                x += 1
        if onebyone:
            for x in range(len(self.populationsize)):
                self.populationsize[x] = coparer[x] if coparer[x].getperformance() > self.populationsize[
                    x].getperformance() else self.populationsize[x]
        elif testself:
            self.populationsize.sort(key=self.sortfunc, reverse=True)
        else:
            print("comparing")
            orgleng = len(self.populationsize)
            self.populationsize.extend(coparer)
            self.populationsize.sort(key=self.sortfunc, reverse=True)
            self.populationsize = self.populationsize[:orgleng]
        time.sleep(1)

    def evaluateindividum(self, individum: NeuralNet):
        self.manager.setbotcount(1)
        self.manager.setstart()
        t = Communicator.Communicator(self.startport, self.address, individum)
        time.sleep(self.runtime)
        self.manager.setstop()
        print("setstop")
        t.setstoprunning()
        t.stopthread()
        t.running = False
        del t
        results = False
        while not results:
            print("waiting for results")
            results = self.manager.checkresults()
        results = self.manager.getresults()
        while results is None:
            #     self.manager.askforresult()
            print("Waitingforresults")
            #    time.sleep(1)
            results = self.manager.getresults()
        print(results)
        individum.setperfromance(results[0])
        self.populationsize.append(individum)
        self.populationsize.sort(key=self.sortfunc, reverse=True)
        del self.populationsize[-1]
        time.sleep(1)

    def DE(self, crossoverrate, scalingfactor: float, first=True, newrun=True):
        self.findbestindividuals(self.populationsize, testself=True)
        if newrun:
            self.writedata()
        while True:
            if not first:
                print("testig old stock")
                self.manager.setshuffle()
                self.findbestindividuals(self.populationsize, testself=True)
            first = False
            x1 = random.randint(0, len(self.populationsize) - 1)
            x2 = random.randint(0, len(self.populationsize) - 1)
            while x1 == x2:
                x1 = random.randint(0, len(self.populationsize) - 1)
                x2 = random.randint(0, len(self.populationsize) - 1)
            scaledpopulation: List[NeuralNet] = []
            self.lowerandupperbound()
            for x in self.populationsize:
                scal1 = (scalingfactor * (
                        self.populationsize[0].getencoded() - x.getencoded()))
                scal2 = (scalingfactor * (
                        self.populationsize[x1].getencoded() - self.populationsize[x2].getencoded()))
                newencoded = x.getencoded() + scal1 + scal2

                index = 0
                for t in x.getencoded():
                    trand = random.random()
                    if not (trand < crossoverrate or trand == t):
                        newencoded[index] = t
                    index += 1

                scaledpopulation.append(
                    NeuralNet(self.layer, newencoded, self.upperbound, self.lowerbound,
                              elter=[0, x1, x2, self.populationsize.index(x)], age=self.iteration))
            self.findbestindividuals(scaledpopulation, True)
            self.writedata()

    def CenDEDOL(self, crossoverrate, scalingfactor: float, bestsolutions, first=True, newrun=True):
        # print(len(self.populationsize[0].getencoded()))
        # print(len(self.populationsize[0].getencoded()))
        self.findbestindividuals(self.populationsize, testself=True)
        if newrun:
            self.obl()
            self.writedata()
        while True:
            if not first:
                print("testig old stock")
                self.manager.setshuffle()
                self.findbestindividuals(self.populationsize, testself=True)
            first = False
            x1 = random.randint(0, len(self.populationsize) - 1)
            x2 = random.randint(0, len(self.populationsize) - 1)
            while x1 == x2:
                x1 = random.randint(0, len(self.populationsize) - 1)
                x2 = random.randint(0, len(self.populationsize) - 1)
            scaledpopulation: List[NeuralNet] = []
            self.lowerandupperbound()
            for x in self.populationsize:
                scal1 = (scalingfactor * (
                        self.populationsize[0].getencoded() - x.getencoded()))
                scal2 = (scalingfactor * (
                        self.populationsize[x1].getencoded() - self.populationsize[x2].getencoded()))
                newencoded = x.getencoded() + scal1 + scal2

                index = 0
                for t in x.getencoded():
                    trand = random.random()
                    if not (trand < crossoverrate or trand == t):
                        newencoded[index] = t
                    index += 1

                scaledpopulation.append(
                    NeuralNet(self.layer, newencoded, self.upperbound, self.lowerbound,
                              elter=[0, x1, x2, self.populationsize.index(x)], age=self.iteration))
            self.findbestindividuals(scaledpopulation, True)
            if random.random() < self.jumpingrate:
                print("doing opposite")
                self.obl()
            else:
                print("doing centered")
                self.lowerandupperbound()
                net = np.zeros(len(self.populationsize[0].getencoded()))
                for i in range(bestsolutions):
                    net += self.populationsize[i].getencoded()
                t = NeuralNet(self.layer, net / bestsolutions, upperbound=self.upperbound, lowerbound=self.lowerbound,
                              elter=[0, 1, 2], age=self.iteration)
                self.evaluateindividum(t)
            self.writedata()
