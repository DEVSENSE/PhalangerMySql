MySQL Managed Extension for Phalanger - the PHP language compiler for the .NET framework - is a full C# reimplementation of the PHP native extension php_mysql. It uses GNU MySQL Connector/.NET and so works with the latest MySQL servers.

https://github.com/DEVSENSE/Phalanger - Phalanger project repository.

Instructions:
Add following XML configuration into your machine.config or web.config, into section configuration/phpNet/classLibrary:
<!-- MySQL extension -->
<add assembly="PhpNetMySql, Version=3.0.0.0, Culture=neutral, PublicKeyToken=2771987119c16a03" section="mysql"/>
<!-- MySQL PDO driver extension -->
<add assembly="PhpNetPDOMySql, Version=3.0.0.0, Culture=neutral, PublicKeyToken=2771987119c16a03" section="pdomysql"/>

Notice: remove usage of native mysql extension, if you were using it.