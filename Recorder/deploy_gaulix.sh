#!/bin/bash

server="valentin@flowapps.link"
port=2234

set -e
set -x

rm -Rf ../published-Recorder/*
dotnet publish --no-restore --no-self-contained --nologo --output ../published-Recorder/

rsync -e "ssh -p $port" -r --info=progress2 ../published-Recorder/ $server:/mnt/containers/meshtastic-mqtt-explorer/Recorder/

ssh $server -t -p $port "docker restart meshtastic-mqtt-explorer-meshtastic_mqtt_recorder-1 && docker logs -f --tail 10 meshtastic-mqtt-explorer-meshtastic_mqtt_recorder-1"
