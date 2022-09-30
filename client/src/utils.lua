utils = {}

function utils.mouseX()
    local x, y = push:toGame(lm.getX(), lm.getY())

    return x or 0
end

function utils.mouseY()
    local x, y = push:toGame(lm.getX(), lm.getY())

    return y or 0
end

function utils.mousePosition()
    local x, y = push:toGame(lm.getX(), lm.getY())

    return x or 0, y or 0
end
