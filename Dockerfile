FROM mono:4.8.0.520

RUN curl -sL https://deb.nodesource.com/setup_5.x | bash - \
    && apt-get install -y nodejs

WORKDIR /app

COPY . /app

CMD ["sh", "/app/build.sh"]

EXPOSE 8083

CMD ["mono", "/app/build/CafeApp.Web.exe"]
