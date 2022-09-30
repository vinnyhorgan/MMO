game = {}

function game:enter()
    self.joystick = gooi.newJoy({size = 100})

    local panelGrid = gooi.newPanel({x = 0, y = 0, w = gameWidth, h = gameHeight, layout = "game"})
    panelGrid:add(self.joystick, "b-l")
    panelGrid:add(gooi.newButton("Action"), "b-r")

    self.client = sock.newClient("localhost", 6969)
    self.client:enableCompression()
    self.client:setDefaultSendMode("unreliable")

    self.players = {}

    self.client:on("connect", function(data)

    end)

    self.client:on("disconnect", function(data)

    end)

    self.client:on("first-connection", function(players)
        self.players = players
    end)

    self.client:on("update", function(diff)
        for _, player in pairs(diff) do
            local oldPlayer = self.players[player.id]

            if oldPlayer then
                local toTween = {
                    x = player.x,
                    y = player.y
                }

                timer.tween(0.2, oldPlayer, toTween)
            else
                self.players[player.id] = player
            end
        end
    end)

    timer.every(0.1, function()
        local input = false
        local velocity = vector(0, 0)

        if lk.isDown("w") or self.joystick:yValue() < -0.6 then
            velocity.y = -1
            input = true
        elseif lk.isDown("s") or self.joystick:yValue() > 0.6 then
            velocity.y = 1
            input = true
        end

        if lk.isDown("a") or self.joystick:xValue() < -0.6 then
            velocity.x = -1
            input = true
        elseif lk.isDown("d") or self.joystick:xValue() > 0.6 then
            velocity.x = 1
            input = true
        end

        if input then
            self.client:send("input", {
                velocity.x,
                velocity.y
            })
        end
    end)

    self.client:connect()

    self.id = self.client:getConnectId()
end

function game:update(dt)
    timer.update(dt)

    gooi.update(dt)

    self.client:update()
end

function game:draw()
    push:start()

    lg.print("PING: " .. self.client:getRoundTripTime(), 10, 10)
    lg.print("PERSONAL ID: " .. self.id, 10, 50)

    for _, player in pairs(self.players) do
        lg.print(player.id, player.x - lg.getFont():getWidth(player.id) / 2, player.y - 20)
        lg.rectangle("fill", player.x, player.y, 25, 25)
    end

    gooi.draw()

    push:finish()
end

function game:keypressed(key, scancode, isrepeat)
    gooi.keypressed(key, scancode, isrepeat)
end

function game:keyreleased(key, scancode)
    gooi.keyreleased(key, scancode)
end

function game:mousepressed(x, y, button)
    gooi.pressed()
end

function game:mousereleased(x, y, button)
    gooi.released()
end

function love.textinput(text)
    gooi.textinput(text)
end
