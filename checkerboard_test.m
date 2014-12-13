realBoardSize = [6 8];

clear cam
cam = webcam;

for k=1:1000
    img = snapshot(cam);
    img = imresize(img, 0.25);

    %Get the checkerboard points.
    [imagePoints,boardSize] = detectCheckerboardPoints(img);

    %Show the image.
    figure(1),imshow(img,[]);
    
    %Make sure the captured image is the correct size.
    if (boardSize(1) == realBoardSize(1) && ...
        boardSize(2) == realBoardSize(2))
    
        hold on;
        %Find the points at the four inner corners.
        corners = [imagePoints(1,:) ; ...
                   imagePoints(boardSize(1)-1,:) ; ...
                   imagePoints(end-(boardSize(1)-2),:) ; ...
                   imagePoints(end,:)];
    
        %Show the inner corners.
        plot(corners(:,1), corners(:,2), '+R');
        hold off;
    end
    
end