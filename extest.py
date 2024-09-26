# Extorr Residual Gas Analyzers DataBase Edition
# (C) Ioffe inst., Igor Bocharov
# Test bulk get parameters

# seems device stuck if request somme of params

import serial
import datetime
import time

try:
    serial = serial.Serial(port="COM4", baudrate=115200, bytesize=8, timeout=2, stopbits=serial.STOPBITS_ONE)
except Exception as e: 
    print(e)
    exit()

set_pirani  = ["PiraniTorr", "PiraniOhms", "PiraniVolts", "PiraniCorrVolts", "PiraniTempVolts"]
set_totpres = ["PressureAmps", "PressureTorr", "PressurePascal"]
set_ionizer = ["SourceGrid1Ma", "SourceGrid2Ma", "IonizerVolts", "IonizerAmps"] #, "IonizerOhms"] NOK!!!
set_filament= ["FilamentPowerPct", "FilamentDacCoarse", "FilamentDacFine"]
set_aux     = ["RfAmpVolts", "ReferenceVolts", "GroundVolts", "FbPlus", "FbMinus", "Focus1FB", "RepellerVolts"]
set_common  = ["QuadrupoleDegC", "InteriorDegC", "SupplyVolts", "DegasMa"]
set_hlam    = ["FilamentStatus" "IsIdle", "LastSweep" "FirstSweep"]

i=0
parl = set_ionizer
print("Extorr RGA extensive polling test", datetime.datetime.now().strftime('%H:%M:%S'))
print(parl)

while 1:

    i+=1
    if (i%60==0): print ("\n - - - - " +(str)(i/60) + " min - - - -")
    print ( " [" + str(i) +"]", end='', flush=True)

    for par in parl:
        req = str.encode( str('get:' + par + '\n\r'))
        serial.write(req)

        ret = serial.readline().decode().rstrip('\n')

        if (ret.find("ok:") == 0):
            val = str (ret.split(":")[2])
            if (len(val)>4):
                print ( "."+val[4], end='')
            else:
                print ( "\n("+ret+")\n", end='')

        time.sleep(0.2)


'''
-> outputs

<- ok:PiraniTorr:4.914e-1
<- ok:PiraniVolts:-2.968
<- ok:PiraniOhms:2.183e4
<- ok:PiraniCorrVolts:-2.966
<- ok:PiraniTempVolts:-2.785

<- ok:PressureAmps:8.366e-15
<- ok:PressureTorr:8.366e-13
<- ok:PressurePascal:1.115e-10

<- ok:SupplyVolts:24.14
<- ok:QuadrupoleDegC:657.3
<- ok:InteriorDegC:24.59

<- ok:SourceGrid1Ma:8.650e-4
<- ok:SourceGrid2Ma:2.258e-4
<- ok:IonizerVolts:7.110e-2
<- ok:IonizerAmps:1.253e-3
<- ok:IonizerOhms:56.75 //!!!

<- ok:FilamentPowerPct:5.469
<- ok:FilamentDacCoarse:2768
<- ok:FilamentDacFine:2047


<- ok:RfAmpVolts:0.0
<- ok:FbPlus:0.0
<- ok:FbMinus:0.0
<- ok:Focus1FB:-20.14
<- ok:RepellerVolts:-68.14
<- ok:ReferenceVolts:2.523
<- ok:GroundVolts:1.725e-2
<- ok:DegasMa:3.841e-1

<- ok:FilamentStatus:0
<- ok:IsIdle:1
<- ok:LastSweep:0
<- ok:FirstSweep:0
'''