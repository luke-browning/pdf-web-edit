#!/bin/bash

# Create mount folders and download sample files
mkdir -p samples volumes/archive volumes/config volumes/inbox volumes/outbox
wget -O samples/colours.jpg https://www.northlight-images.co.uk/wp-content/uploads/2021/12/colours_srgb.jpg
wget -O samples/dummy.pdf https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf
cp samples/dummy.pdf volumes/inbox/sample.pdf