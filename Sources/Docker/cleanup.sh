#!/bin/bash
COLOR_REST="$(tput sgr0)"
COLOR_RED="$(tput setaf 1)"
COLOR_GREEN="$(tput setaf 2)"
COLOR_BLUE="$(tput setaf 4)"

if (! docker stats --no-stream &> /dev/null); then
  if [[ "$(uname)" == "Darwin" ]]; then
    # On Mac OS this would be the terminal command to launch Docker
    open /Applications/Docker.app
  elif [[ "$(uname -s)" == *"MINGW64_NT"* ]]; then
    printf '%s%s%s\n' $COLOR_RED "Make sure Docker Desktop is running and restart this script" $COLOR_REST
    echo "C:\Program Files\Docker\Docker\Docker Desktop.exe"
    exit 1
  fi
  
  printf '%s%s%s\n' $COLOR_BLUE "Waiting for Docker to launch..." $COLOR_REST
  # Wait until Docker daemon is running and has completed initialisation
  while (! docker stats --no-stream &> /dev/null); do
    printf '%s%s%s' $COLOR_GREEN '..' $COLOR_REST
    # Docker takes a few seconds to initialize
    sleep 2
  done
fi

docker builder prune --filter 'until=8h' -f
docker image prune --filter 'until=8h' -f
docker container prune --filter "until=8h" -f

# when force everything
# docker system prune -a -f

pushd ../Sources

    MAIL_HOG_DATA=ThirdParty/services/mailhog/data
    if [[ -d "$MAIL_HOG_DATA" ]]
    then
        echo "Clean MailHog $MAIL_HOG_DATA exists on your filesystem."
        pushd "$MAIL_HOG_DATA"
            rm *mailhog.example
        popd
    fi
    
    RABBIT_MQ_MNESIA=ThirdParty/services/rabbitmq/data/mnesia
    if [[ -d "$RABBIT_MQ_MNESIA" ]]
    then
        echo "Clean rabbitmq mnesia $RABBIT_MQ_MNESIA exists on your filesystem."
        pushd "$RABBIT_MQ_MNESIA"
            rm -rf rabbit*
        popd
    fi
    
    RABBIT_MQ_LOGS=ThirdParty/services/rabbitmq/logs
    if [[ -d "$RABBIT_MQ_LOGS" ]]
    then
        echo "Clean rabbitmq logs $RABBIT_MQ_LOGS exists on your filesystem."
        pushd "$RABBIT_MQ_LOGS"
            rm rabbit*
        popd
    fi

popd