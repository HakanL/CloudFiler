Simple file management for S3 compatible cloud storage providers (like OpSource Cloud Files)
Provides functions to:
Upload files
Download files
Delete remote files


Example usage:
CloudFiler.exe --accessKey=aaaabbbbbcccc44455566 --accessSecret=xxxyyyyzzzz111222333 --server=interop-na1.opsourcecloud.net --bucket=temp GET Filename.rar

Where you get the AccessKey and AccessSecret from your Storage Provider. The bucket is typically the directory name.
Commands:
GET
PUT
DELETE

And it can take multiple files at the end of the command line
