ReliablePubSub
==============

Source code for Reliable PubSub from http://somdoron.com/2015/09/reliable-pubsub.

So what Reliable PubSub really does:

* Trying to connect to multiple servers and pick the first one that reply with Welcome Message. Can be used as geo load balancer or to connect to less busy server.
* Recognize temporary connection drop (when Welcome Message arrived while the connection is up), the user can use that to request full snapshot, take a look at the Clone pattern from zeromq guide.
* Reconnect when the server is dead, will probably connect to the next closest server.

The Reliable PubSub is a good fit for:

* Financial Market Data (Forex/Stock Quotes)
* Social Stream
* Publishing changes to a client

If you implement the pattern in another language please send it to me and I will add a link to the post.
