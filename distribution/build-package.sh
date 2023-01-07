# Note, this script is only used for local dev tests, this is not the script used for building the official whisparr package

mkdir -p /${PWD}/../_output_debian

docker build -f docker-build/Dockerfile -t whisparr-packager ./docker-build

docker run --rm -v /${PWD}/../_output_linux:/data/whisparr_bin:ro -v /${PWD}:/data/build -v /${PWD}/../_output_debian:/data/output whisparr-packager
