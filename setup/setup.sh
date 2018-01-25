#!/bin/bash

set -e

thisdir="$(dirname "$0")"
source "$thisdir/functions.sh"

echo "Setting up $thisdir"

for f in $(find "$thisdir/../parts/" -name setup.sh -mindepth 2 -maxdepth 2); do
  libsrc="$(dirname $f)"
  echo
  echo "############ BEGIN: $f"
  source $f
  echo "############ DONE: $f"
  echo
done

setup_gitignore

echo "Done with $thisdir"
