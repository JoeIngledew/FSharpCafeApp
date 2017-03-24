FROM mono:4.8.0.495

RUN curl -sL https://deb.nodesource.com/setup_5.x | bash - \
    && apt-get install -y nodejs

WORKDIR /app

COPY . /app

RUN ls

RUN chmod +x ./monobuild.sh

RUN sh ./monobuild.sh

EXPOSE 8083

CMD ["mono", "/app/build/CafeApp.Web.exe"]
