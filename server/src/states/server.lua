server = {}

function server:enter()
    self.server = sock.newServer("*", 6969)
    self.server:enableCompression()
    self.server:setDefaultSendMode("unreliable")

    self.diff = {}
    self.players = {}

    self.server:on("connect", function(data, client)
        local id = client:getConnectId()

        local newPlayer = {
            id = id,
            x = 100,
            y = 100
        }

        self.players[id] = newPlayer

        client:send("first-connection", self.players)

        table.insert(self.diff, newPlayer)

        print(id .. " connected!")
    end)

    self.server:on("disconnect", function(data, client)
        local id = client:getConnectId()

        self.players[id] = nil

        print(id .. " disconnected...")
    end)

    self.server:setSchema("input", {
        "x",
        "y"
    })

    self.server:on("input", function(data, client)
        local id = client:getConnectId()
        local player = self.players[id]

        player.x = player.x + data.x * 30
        player.y = player.y + data.y * 30

        table.insert(self.diff, player)
    end)

    timer.every(0.1, function()
        self.server:sendToAll("update", self.diff)
        self.diff = {}
    end)

    self.prevTotalData = self.server:getTotalReceivedData() + self.server:getTotalSentData()
    self.dataPerSecond = 0

    timer.every(1, function()
        local newTotalData = self.server:getTotalReceivedData() + self.server:getTotalSentData()
        self.dataPerSecond = newTotalData - self.prevTotalData
        self.prevTotalData = newTotalData
    end)
end

function server:update(dt)
    timer.update(dt)

    self.server:update()
end
